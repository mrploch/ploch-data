# Context

The read repositories expose two ways to influence a query: a `query` parameter of type
`Expression<Func<TEntity, bool>>?` for *filtering*, and an `onDbSet` parameter of type
`Func<IQueryable<TEntity>, IQueryable<TEntity>>?` for *shaping* — eager loading with `Include` /
`ThenInclude`, ordering, `AsNoTracking`. Because `onDbSet` receives the raw `IQueryable`, nothing
stopped callers from smuggling a `.Where(...)` through it, and the library's own tests did exactly
that. ([#77](https://github.com/mrploch/ploch-data/issues/77))

An audit of the whole read surface found the asynchronous interface already correct *on this axis* —
every method on `IReadRepositoryAsync<TEntity>` that can filter takes a separate `query`. (Two
unrelated pre-existing divergences between the two surfaces were found during review and filed
separately as [#104](https://github.com/mrploch/ploch-data/issues/104); they are out of scope here.)
The divergence this change addresses was on the **synchronous** `IReadRepository<TEntity>`, which had
never been brought in line:

| Method | `query` | `sortBy` | Notes |
|---|:--:|:--:|---|
| `GetAllAsync` / `GetPageAsync` / `CountAsync` / `FindFirstAsync` | yes | n/a / yes | already correct |
| `GetAll` | **no** | n/a | only `onDbSet` was offered |
| `Count` | **no** | n/a | no way to count a subset |
| `GetPage` | yes | **no** | diverged from `GetPageAsync` |

`GetById` / `GetByIdAsync` are a deliberate exception: they look an entity up by its primary key and
take no `query` parameter at all.

# Change

- `IReadRepository<TEntity>.GetAll` gained a leading `query` parameter, matching `GetAllAsync`:
  `GetAll(Expression<Func<TEntity, bool>>? query = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null)`.
- `IReadRepository<TEntity>.Count` gained an optional `query` parameter, matching `CountAsync`.
- `IReadRepository<TEntity>.GetPage` gained a `sortBy` parameter **at position 3**, giving it the
  same parameter order as `GetPageAsync`:
  `GetPage(int pageNumber, int pageSize, Expression<Func<TEntity, object>>? sortBy = null, Expression<Func<TEntity, bool>>? query = null, Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null)`.
- `ReadRepository<TEntity>` implements all three, mirroring the asynchronous implementations.
  `GetPage` forwards to the existing `GetPageQuery`, which already applied `sortBy`, `query`, and
  `onDbSet` in the right order.
- XML documentation on **every** method carrying `onDbSet` — synchronous and asynchronous, on both
  the interfaces and the EF Core implementations — now states the contract explicitly: shaping only,
  never filtering. `GetById` / `GetByIdAsync` point callers at `FindFirst` / `FindFirstAsync`
  instead, since they have no `query` parameter of their own, and document that supplying `onDbSet`
  also forces a database round-trip instead of a possible change-tracker hit.

# Impact

**Breaking (v4.0).** Three of the four consequences below are compile-time breaks. The fourth — a
three-argument positional `GetPage` call — still compiles and changes meaning; it is called out
separately under "One call shape rebinds silently" and is the one to audit for.

1. **Positional `GetAll` callers.** `GetAll(q => q.Include(...))` no longer compiles — the first
   parameter is now the predicate. Fix by naming the argument: `GetAll(onDbSet: q => q.Include(...))`.
   `GetAll` has no silent-rebind hazard at all: a lambda valid as the old `onDbSet` has body type
   `IQueryable<TEntity>` and a lambda valid as the new `query` has body type `bool`, so no lambda
   satisfies both and every positional migration is a hard compile error. `null` and `default` in
   first position bind to `query` instead of `onDbSet` and produce an identical result set.
2. **Positional `GetPage` callers.** `GetPage(1, 20, predicate, q => q.Include(...))` no longer
   compiles — the fourth positional argument lands on `query` and the shaping lambda does not
   convert to it. Fix by naming the arguments. **Not every positional `GetPage` call fails loudly,
   though** — see the section below.
3. **Custom `IReadRepository<TEntity>` implementations** must update the three changed signatures.
   This is a source break for implementors and a binary break for already-compiled assemblies:
   `Count()` and `Count(Expression<Func<TEntity, bool>>?)` are different metadata methods even
   though source calls to `Count()` still compile.

### ⚠️ Some `GetPage` calls rebind silently — how to audit for them

`sortBy` is `Expression<Func<TEntity, object>>`, and a `bool`-bodied **lambda** converts to it
implicitly (the `bool` boxes). So a pre-4.0 predicate passed *positionally in third position* now
binds to `sortBy` instead of `query`, and the call still compiles:

````csharp
// Before 4.0 — filters to published posts.
// Since 4.0  — orders by a boxed bool and returns EVERY row, unfiltered.
repository.GetPage(1, 20, post => post.IsPublished);

// Fix — name the argument.
repository.GetPage(1, 20, query: post => post.IsPublished);
````

**The rule is about the third argument, not the argument count.** Any `GetPage` call whose third
argument is passed positionally *and is a lambda* rebinds silently, regardless of how many arguments
follow or whether they are named. All of these compile and change meaning:

````csharp
repository.GetPage(1, 20, post => post.IsPublished);                                  // 3 positional
repository.GetPage(1, 20, post => post.IsPublished, null);                            // 4 positional
repository.GetPage(1, 20, post => post.IsPublished, default);                         // 4 positional
repository.GetPage(1, 20, post => post.IsPublished, onDbSet: q => q.Include(...));    // 3 positional + named
repository.GetPage(1, 20, post => post.IsPublished, onDbSet: shapeVariable);          // 3 positional + named
````

The fourth form deserves particular attention: it is the style this library's own documentation and
tests encouraged before 4.0, so it is the shape most likely to exist in consumer code.

These two forms, by contrast, **fail to compile** — they are safe:

````csharp
Expression<Func<Post, bool>> predicate = post => post.IsPublished;
repository.GetPage(1, 20, predicate);                                   // CS1503 — Expression<T> is invariant
repository.GetPage(1, 20, post => post.IsPublished, q => q.Include(...)); // CS0411 — 4th positional lambda
````

So: **grep for `GetPage(` with a positional third argument that is a lambda; the argument count is
irrelevant.** Calls that pass a typed `Expression<Func<T, bool>>` variable, or that already name
their arguments, are unaffected. `GetAll` and `Count` have no equivalent hazard — see below.

# Design notes

- **`sortBy` went to position 3 rather than the end.** Appending it would have kept source
  compatibility, but would have left the synchronous and asynchronous signatures permanently
  disagreeing on parameter order — the exact drift that produced this issue. A major version is the
  right place to pay that cost once.
- **No `[Obsolete(error: true)]` migration tripwire was added.** A legacy overload was prototyped
  and empirically tested against the C# compiler. Making it fire on the hazardous positional call
  *without* also breaking correct named-argument calls required its third parameter to be
  mandatory **and** its parameter names to differ from the real ones — otherwise
  `GetPage(1, 20, query: ...)` became `CS0121` (ambiguous) and
  `GetPage(1, 20, query: ..., onDbSet: ...)` hard-errored on correct code, because the legacy
  overload wins the "fewer declared parameters" tie-break. Protecting concrete-typed callers as
  well as interface-typed ones needed a second copy on `ReadRepository<TEntity>`, since a default
  interface implementation does not participate in a class's member lookup. Two obsolete members
  with deliberately-wrong parameter names, to be deleted in 4.1, was judged a worse trade than
  documenting the single affected call shape.
- **`GetById` / `GetByIdAsync` keep their signatures.** A lookup by primary key is a complete
  contract; an extra predicate belongs on `FindFirst` / `FindFirstAsync`. The `onDbSet` parameter
  stays because eager loading a by-id lookup is a genuine and common need.

# Tests

`sortBy` had **no test coverage at all** before this change — on `GetPageQuery`, `GetPageAsync`, or
anywhere else. That gap is now closed.

Five tests were filtering through `onDbSet` and have been rewritten so the shaping they pass is
something `onDbSet` is actually for, with the filtering moved to `query`:

- `ReadRepositoryTests.Find_with_OnDbSet_action_should_apply_the_shaping_to_the_query` — the
  predicate now matches three rows so the `OrderByDescending` passed through `onDbSet` decides the
  result, making the shaping observable.
- `ReadRepositoryTests.GetPage_should_return_a_page_of_entities_with_includes_using_query` — filter
  in `query`, ordering in the new `sortBy`, `onDbSet` left doing only eager loading.
- `QueryableRepositoryTests.GetPageQuery_with_onDbSet_should_apply_the_shaping_to_the_query` — the
  `Where` became an `OrderByDescending`; the page size now does the narrowing.
- `ReadWriteRepositoryAsyncAdditionalTests.GetByIdAsync_with_onDbSet_should_apply_the_shaping_to_the_query`
  — asserts `AsNoTracking` leaves the returned entity `Detached`. Ordering is not observable for a
  by-id lookup, so shaping is demonstrated through tracking behaviour instead.
- Two `..._should_return_null_when_filter_excludes_entity` tests were **deleted** rather than
  rewritten: they asserted that a filter passed through `onDbSet` could suppress a by-id hit, which
  is precisely the behaviour this change documents as unsupported. Deleting them would have left the
  `onDbSet != null` branch of `GetById`/`GetByIdAsync` — which resolves through
  `onDbSet(DbSet).FirstOrDefault(e => Equals(e.Id, id))` rather than `DbSet.Find(id)` — with no
  not-found coverage at all, since the existing `..._should_return_null_when_entity_does_not_exist`
  tests take the null-`onDbSet` branch. Replacements covering that branch were added for both the
  sync and async repositories, shaping with `AsNoTracking` and letting the missing id produce the
  null.

Six tests added for the widened surface:

- `GetAll_should_filter_the_entities_using_the_query`
- `GetAll_should_apply_the_query_and_the_shaping_together` — proves `query` narrows *and* `onDbSet`
  eager-loads in the same call
- `Count_should_count_only_the_entities_matching_the_query`
- `GetPage_should_order_the_results_using_sortBy`
- `GetPageQuery_with_sortBy_should_order_the_results`
- `GetByIdAsync_should_return_null_when_the_id_does_not_exist`

Full suite: 218 tests across 8 projects, all passing.
