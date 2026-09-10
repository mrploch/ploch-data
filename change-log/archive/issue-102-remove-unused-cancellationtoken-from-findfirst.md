# Issue #102 — Remove the unused `CancellationToken` from synchronous `IReadRepository.FindFirst`

## Context

`IReadRepository<TEntity>.FindFirst` declared a `CancellationToken` parameter that its EF Core
implementation never used:

```csharp
public TEntity? FindFirst(Expression<Func<TEntity, bool>> query,
                          Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null,
                          CancellationToken cancellationToken = default)
    => onDbSet == null ? DbSet.FirstOrDefault(query) : onDbSet(DbSet).FirstOrDefault(query);
```

The synchronous `Queryable.FirstOrDefault` has no overload that accepts a token, so there was nowhere
for it to go. A caller that passed one reasonably expected cancellation to be honoured and received
nothing — no cancellation, no exception, no warning.

It was also inconsistent within its own interface: `FindFirst` was the **only** synchronous method on
`IReadRepository<TEntity>` that advertised cancellation. `GetAll`, `GetPage`, `Count` and `GetById`
never took a token.

Found during the review of #77, which reworked the neighbouring signatures on this same interface, and
deliberately left out of scope there to keep that change focused on the `query` / `onDbSet` separation.

## Change

The parameter is removed from the interface and from `ReadRepository<TEntity>`:

```csharp
TEntity? FindFirst(Expression<Func<TEntity, bool>> query,
                   Func<IQueryable<TEntity>, IQueryable<TEntity>>? onDbSet = null);
```

The XML documentation gained a `<remarks>` block stating plainly that the method is synchronous and
therefore takes no token, and pointing callers who need cancellation at `FindFirstAsync`.

`IReadRepositoryAsync<TEntity>.FindFirstAsync` is **unchanged** — it takes a token and genuinely uses
it, passing it to `FirstOrDefaultAsync`.

## Why removal rather than `ThrowIfCancellationRequested`

The alternative was to keep the parameter and call `cancellationToken.ThrowIfCancellationRequested()`
before executing the query, making an already-cancelled token observable.

Removal was chosen because that halfway position is still misleading: it honours cancellation only in
the instant before the query starts and not at all during it, which is the part that actually takes
time. A caller reading the signature would still reasonably expect cooperative cancellation of the
database round-trip, and still not get it. A synchronous method that advertises cancellation it cannot
deliver is worse than one that never claims to.

## Impact

This is a breaking change, and it lands differently depending on the relationship to the interface:

| Who | Source | Binary |
|---|---|---|
| Direct implementors of `IReadRepository<TEntity>` / `IReadWriteRepository<TEntity, TId>` | **Breaks** — `CS0535`, regardless of whether the token was used | Breaks |
| Callers using `FindFirst(predicate)` or `FindFirst(predicate, onDbSet)` | Compiles unchanged | **Breaks until recompiled** — optional arguments are baked in at the call site, so existing IL still targets the three-parameter method |
| Callers passing the token positionally or as `cancellationToken:` | **Breaks** — `CS1501` / `CS1739` | Breaks |
| Types deriving from `ReadRepository<TEntity>` / `ReadWriteRepository<TEntity, TId>` without redeclaring | Compiles unchanged | Unaffected — the derived assembly emits no `FindFirst` method of its own and inherits the updated base implementation |
| Mock setups / test doubles written against the three-parameter form | **Breaks** | Breaks |

**Nothing rebinds silently.** This is the important contrast with the `GetPage` change in #77, where a
positional predicate could bind to the new `sortBy` parameter and change behaviour without a compiler
error. Nothing here keeps compiling while quietly changing meaning.

Note the precise shape of that claim: it does *not* mean every affected site fails to compile. Source
breaks — direct implementors, and callers that pass a token — fail the build outright. Callers using the
one- or two-argument form still compile from source and are affected only at the binary level: their
existing IL targets the three-parameter method, so they must be rebuilt against 4.0. That is ordinary
for a major version, and it cannot produce wrong behaviour — only a `MissingMethodException` if an old
binary is run against the new assembly without recompiling.

Implementors must delete the parameter from their own signature.

v4.0.0 is the correct window: the same release already breaks this interface family via #77, and
`IAuditEntityHandler` via #88.

## Scope of the sweep

A grep across the entire 20-repository workspace found every reference:

| Site | Action |
|---|---|
| `src/…/IReadRepository.cs` | Parameter and its `<param>` doc removed; `<remarks>` added; unused `using System.Threading;` dropped |
| `src/…/ReadRepository.cs` | Parameter removed; unused `using System.Threading;` dropped |
| `tests/…/ServiceCollectionRegistrationsTests.cs` | `CustomBlogRepository` stub signature updated — this is the implementor compile-check |
| `docs/GenericRepositories.svg` | Stale signature text corrected; `textLength` attribute dropped so the shortened text is not letter-spaced |

`ReadWriteRepository<TEntity, TId>` inherits the method and does not override it. The two real call
sites in `ReadRepositoryTests` never passed a token and needed no change. **No sibling repository in the
workspace consumes the synchronous `FindFirst` at all.**

## Tests

No new tests. No executable code changed: the removed parameter was never read, so for any caller
recompiled against 4.0 the query behaviour is identical. (That is a statement about *query* behaviour,
not about every runtime outcome — an old binary run against the new assembly without recompiling fails
with `MissingMethodException`, as described above.) A test asserting that a parameter no longer exists
would only be re-testing the compiler.

The existing coverage is what matters and it still passes:

- `ReadRepositoryTests.Find_should_query_repository_for_first_entity_and_return_it` and
  `Find_with_OnDbSet_action_should_apply_the_shaping_to_the_query` — behaviour of `FindFirst`.
- `ServiceCollectionRegistrationsTests` — compiles the `CustomBlogRepository` stub, which is the real
  guard here: if the interface and the implementation had drifted apart, this project would not build.

216 tests pass across 7 test projects, 0 failures, 0 skipped.
