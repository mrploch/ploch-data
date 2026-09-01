# Release Notes

## Unreleased

### Changed behaviour (no API break)

- **`GenericRepositoryDataIntegrationTest<TDbContext>`: `useScopedProvider: false` now means "a new
  scope", not "the root container".** Every signature is unchanged, so this is not a compile-time
  break, but the observable behaviour of the `false` overloads moves for consumers of
  `Ploch.Data.GenericRepository.EFCore.IntegrationTesting`. A downstream test that resolved a
  repository with `false` previously got a root-cached instance sharing the test's `DbContext` and
  change tracker; it now gets an independent `DbContext` and will no longer observe entities that
  are tracked but not yet saved. If a test relied on that sharing, either drop the `false` argument
  to use the shared scope or save before reading. No test inside this repository passed `false`.
  (#80)

### Added

- **`TestDbContextHarness<TDbContext>`** (`Ploch.Data.EFCore.IntegrationTesting`) — a single
  `IDisposable`/`IAsyncDisposable` owner for everything an integration-test database build creates:
  the root service provider, the initial service scope, the initial `DbContext` and — on the
  connection-string overload, which creates it — the shared SQLite connection. On the
  `IDbContextConfigurator` overload the connection belongs to the configurator, so the caller still
  disposes the configurator; `DataIntegrationTest<TDbContext>` does that for you. Disposal is
  resilient: one failing resource does not strand the rest, and failures are rethrown afterwards.
  Obtain a harness from the new `DbContextServicesRegistrationHelper.BuildHarness<TDbContext>(…)`
  overloads. The existing `BuildDbContextAndServiceProvider` tuple overloads are unchanged and now
  route through the harness, so there is one construction path; they still hand back references
  without ownership, so prefer `BuildHarness`. (#80)

- **`DataIntegrationTest<TDbContext>.CreateScope()`** — creates a dependency-injection scope from
  the root provider and disposes it with the test. Use it when a test needs genuinely independent
  scoped services, such as a second `DbContext` with its own change tracker. (#80)

### Fixed

- **`GenericRepositoryDataIntegrationTest` no longer resolves scoped services from the root
  container** — `CreateUnitOfWork(useScopedProvider: false)` and the `Create…Repository…(false)`
  overloads returned services resolved straight from the root provider. Repositories and
  `IUnitOfWork` are registered as scoped, so root resolution promoted them to de-facto singletons
  and shared one `DbContext` — and its change tracker — across operations meant to be independent.
  They now resolve from a fresh scope that is disposed with the test. The `useScopedProvider`
  parameter is deliberately retained, so this is a behavioural fix, not an API break. `DbContext`,
  `ScopedServiceProvider` and `RootServiceProvider` additionally throw `InvalidOperationException`
  naming the initialisation requirement when the harness has not been built. (#80)

- **Cross-repo `ploch-common` references are now consistent in every configuration** — Release
  builds resolved `Ploch.Common` from the 3.x package feed while the test projects resolved the
  unpublished `Ploch.TestingSupport.XUnit3.*` projects (and therefore `Ploch.Common` 4.0.x) from the
  sibling checkout. The two generations landed in the same output folder and the TestingSupport
  assemblies failed to bind, making `dotnet test -c Release` red. `UseProjectReferences` now
  defaults to `true` in all configurations; pass `-p:UseProjectReferences=false` to restore
  `PackageReference` resolution. Because a `ProjectReference`-resolved pack would write the sibling
  checkout's `Ploch.Common` version into the nuspec — a version that does not exist on nuget.org —
  `release.yml` and `deploy-nuget-org.yml` now pack in a dedicated step with
  `-p:UseProjectReferences=false` and publish only from that output, so the shipped packages keep
  declaring the central `Ploch.Common` versions. Pinning the `ploch-common` clone to a released tag
  remains tracked in #67. (#95)
- **`CollectionStringSplitConverter<TValue>` has a new, tagged wire format that stores what you gave
  it** — four defects are fixed together because all four need the same format revision. An empty
  `string` element is no longer indistinguishable from `null` (`["a", ""]` used to reload as
  `["a", null]`, and both `[""]` and `[null]` collapsed to `[]`); `DateTime` no longer loses sub-second
  precision (`10:30:45.1230000` used to be stored as `10:30:45` — corruption with no exception) or
  `DateTimeKind` (`Utc` and `Local` both read back as `Unspecified`, invisibly, because `DateTime`
  equality compares ticks only); and element types outside `Convert.ChangeType`'s reach — `Guid`,
  enums, `Nullable<T>`, `TimeSpan`, `DateTimeOffset`, `DateOnly`, `TimeOnly` — now round-trip instead
  of serialising successfully and throwing `InvalidCastException` on every read.
  See `change-log/issue-121-converter-tagged-wire-format.md`. (#121)

  **The format.** A non-`null` collection is written as the version header `!1` followed by one
  *separator-introduced* segment per element — including the first — and every segment carries a
  mandatory one-character tag: `n` for a `null` element, or `v` followed by the escaped value.

  | Collection | Payload |
  |---|---|
  | `[]` | `!1` |
  | `[""]` | `!1,v` |
  | `[null]` | `!1,n` |
  | `["a", ""]` | `!1,va,v` |
  | `["a", null]` | `!1,va,n` |
  | `[1, 0, 2]` | `!1,v1,v0,v2` |

  Because every element is introduced by the separator, no element can be encoded as an empty segment,
  so the ambiguity is structurally impossible rather than merely narrowed. `!` is a safe sentinel
  because `Uri.EscapeDataString` output is drawn only from the RFC 3986 unreserved characters
  (`A-Z a-z 0-9 - . _ ~`) and percent-triplets — `!` escapes to `%21`, so escaped element data can
  never begin with the header. The `v`/`n` tags are inside that alphabet but are read *positionally*,
  as the first character of a segment whose boundaries the separator has already fixed, so element data
  spelling `"v"` or `"n"` cannot be mistaken for structure.

  **Element encodings** are now chosen for round-trip fidelity rather than taken from
  `Convert.ToString`: `DateTime` and `DateTimeOffset` use `"O"` (which carries all seven
  fractional-second digits *and* the `Kind`/offset), `TimeSpan` uses `"c"`, `Guid` uses `"D"`,
  `DateOnly`/`TimeOnly` use `"O"`, enums are written by name, and everything else `IConvertible` keeps
  its invariant string form. A `Nullable<T>` element decodes through its underlying type, because the
  `null` case is carried by the tag. A `TValue` outside that set now throws `NotSupportedException`
  **on write**, naming the type, instead of writing something unreadable.

  Support is judged from the **declared** element type, not from the runtime value, so a converter
  closed over an unsupported type fails even when the value it is handed happens to be encodable —
  `CollectionStringSplitConverter<object>` can no longer write a string element it would be unable to
  read back. Implementing `IConvertible` is not by itself sufficient: decoding converts the stored
  `string`, and `string`'s own `IConvertible` implementation recognises only the built-in primitives,
  throwing `InvalidCastException` for a user-defined target. Reading a payload whose `n` tag marks a
  `null` element is likewise rejected when `TValue` cannot represent `null`, rather than quietly
  yielding `default(TValue)`.

  **Breaking change — on-disk format, and legacy payloads are rejected.** A non-`null` payload that
  does not begin with `!1` throws `FormatException`. Reading legacy payloads best-effort was considered
  and rejected: under the old rules an empty segment meant `null` **and** `string.Empty`, so a
  best-effort read would hand back data that is quietly wrong for exactly the inputs this change
  exists to fix — a loud failure is recoverable, a silent misread is not. The blast radius is
  negligible: the previous format never reached a released version, and the format before it could not
  be read back at all (its read path threw `InvalidCastException` for *every* payload and every
  `TValue`), so no data this converter wrote was ever readable. If you do hold rows written by a
  pre-4.0 build on this branch, rewrite the column before upgrading — there is no in-place migration,
  and none could be correct.

  **This supersedes the "Known limitations" noted under the entries below.** Those four items are the
  four fixed here.

- **`CollectionStringSplitConverter<TValue>` no longer throws on a `null` collection** — `convertNulls`
  defaults to `true`, so EF Core invokes the conversion lambdas for nulls instead of short-circuiting
  them, but neither lambda handled null: a `null` collection threw `ArgumentNullException` out of
  `Enumerable.Select` during `SaveChanges`, and a `NULL` column would have thrown
  `NullReferenceException` on read. `null` now maps to `null` in both directions. A side benefit is
  that a `null` collection is now distinguishable from an empty one — the former is a `NULL` column,
  the latter a non-`NULL` one. (In the final 4.0 format an empty collection is the bare `!1` header,
  not the empty string — see the #121 entry above.) (#122)

  **Behavioural change:** a property mapped with this converter must be declared nullable if it is to
  hold `null`; a non-nullable property still maps to a `NOT NULL` column, which correctly rejects it.

- **`CollectionStringSplitConverter<TValue>` now rejects a separator that escaping leaves unchanged**
  — escaping is what keeps a separator occurring *inside* an element from being read as a delimiter,
  but `Uri.EscapeDataString` passes the RFC 3986 unreserved characters (`A-Z a-z 0-9 - . _ ~`)
  through unchanged. With `separator: "-"` the element `"a-b"` was written as `a-b` and read back as
  two elements — silent corruption with no exception. (#123)

  **Breaking change:** the constructor now throws `ArgumentException` for a separator consisting only
  of unreserved characters (for example `"-"`, `"."`, `"_"`, `"~"`, or any alphanumeric string) and
  for an empty separator, and `ArgumentNullException` for a `null` separator. Callers passing such a
  separator were already corrupting any element containing it, so the exception replaces data loss
  rather than working behaviour. The default `","` is unaffected.

- **`CollectionStringSplitConverter<TValue>` now writes with the invariant culture** — values were
  serialised with `ToString()` (current culture) but deserialised with `CultureInfo.InvariantCulture`,
  so round-trips under cultures such as `pl-PL` or `de-DE` corrupted data or threw `FormatException`.
  Both directions now agree on the invariant culture. **Behavioural change:** values are now always
  written invariantly; data previously written under a non-invariant current culture was already
  unreadable by the converter's invariant read path, so the practical impact is positive. The read
  path additionally failed to materialise value-typed collections at all — it cast a lazy
  `Select` iterator of `object` directly to `ICollection<TValue>`, throwing `InvalidCastException`
  the first time an entity was actually loaded from the database rather than served from the
  change tracker; elements are now converted individually and materialised into a list. Two more
  read-path defects fixed in the same pass: the payload was unescaped **before** splitting, so an
  element containing the separator (written escaped, e.g. `%2C`) was torn apart on read — segments
  are now unescaped individually after the split; and an empty payload threw `FormatException` for
  value-typed elements instead of producing an empty collection. An empty *segment* also decodes to
  `default(TValue)` rather than throwing — that path now exists only to read payloads written by
  earlier versions, because the writer no longer produces empty segments for non-`null` elements
  (see the next entry). (#97)

  *Superseded within this same unreleased cycle by #121:* segments are now tagged and the
  empty-segment encoding no longer exists in any form — see the first entry above.

- **A one-element collection holding `default(TValue)` no longer vanishes on read** — the write path
  stored *any* element equal to `default(TValue)` as an empty segment, so a collection of exactly one
  such element serialised to the empty payload, which is indistinguishable from an empty collection.
  `[0]`, `[false]`, `[0m]` and `[default(DateTime)]` all silently reloaded as `[]` — data loss with no
  exception. Elements are now written verbatim rather than being collapsed when they equal the type
  default, so for **value-typed elements** the encoding is now cardinality-preserving: every
  non-`null` value writes at least one character, the writer never emits an empty segment, and an
  empty payload means exactly an empty collection. (Cardinality is preserved, which is not the same
  as every value surviving intact — `DateTime` is a value type and still loses sub-second precision,
  as noted under Known limitations.) Reference-typed elements are unchanged — an empty
  segment still means `null` *or* the empty string, and still reads back as `null` (see Known
  limitations). (#97)

  **Breaking change — on-disk format.** Value-typed collections containing default elements are
  encoded differently: `[1, 0, 2]` was stored as `"1,,2"` and is now stored as `"1,0,2"`;
  `[true, false]` was `"True,"` and is now `"True,False"`. Rows written before and after this change
  therefore use different encodings for the same value, and stored values get slightly longer — which
  matters only for a column with a tight `MaxLength`.

  *Superseded within this same unreleased cycle by #121:* `[1, 0, 2]` is now stored as `"!1,v1,v0,v2"`,
  and untagged payloads are rejected rather than decoded — see the first entry above.

  The read path is unchanged and still decodes an empty segment to `default(TValue)`, so legacy
  *multi-element* payloads such as `"1,,2"` remain readable and still yield `[1, 0, 2]`. Legacy
  *single-default* rows are not recoverable: `[0]` was written as the empty payload, which is
  indistinguishable from an empty collection and still decodes to `[]`. That is the defect being
  fixed, not a regression — and in practice the blast radius is small, because before this release
  the converter's read path threw `InvalidCastException` for *every* payload, so no such data could
  previously be read back at all.

  **Known limitations at the time of that change — all four since fixed by #121, see the first entry
  above; retained here for the history**: an empty `string` element was
  indistinguishable from `null` and read back as `null`; a collection holding a single empty-or-null
  string read back empty; `DateTime` elements lost sub-second precision and `DateTimeKind`, because
  the invariant general format had neither a fractional-seconds field nor an offset; and a `TValue`
  outside `Convert.ChangeType`'s supported set (`Guid`, enums, `Nullable<T>`) serialised but threw
  `InvalidCastException` on read.

### Removed

- **`IAuditEntityHandler.HandleAccess` is gone — reads are not audited** — the method was documented as
  being called whenever an entity was read, and as telling the repository whether the entity had been
  modified "so that the entity can be updated in the data source". Neither held up: synchronous reads never
  called it while asynchronous reads did, the return value was discarded at both call sites, and the shipped
  `AuditEntityHandler.HandleAccess` was hard-coded to `false`, so the feature was documented but never
  implemented. Rather than wire it up, read auditing is dropped: no handler method is invoked on a read path,
  and `AccessedTime` / `LastAccessedBy` are never written by the repositories — set them yourself if you need
  them. `HandleCreation` and `HandleModification` are unchanged.
  See `change-log/issue-104-remove-handleaccess.md`. (#104)

  **Breaking change**, though how it lands depends on how the member was implemented:

  - An **explicit** implementation (`bool IAuditEntityHandler.HandleAccess(object entity)`) fails to compile
    — `CS0539`, the interface no longer has that member. Delete it.
  - An **implicit** implementation (`public bool HandleAccess(object entity)`) keeps compiling. It simply
    stops being an interface implementation and becomes an ordinary public method that nothing ever calls.
    Deleting it is housekeeping, not a compile requirement.
  - Any **direct call** through the interface — `handler.HandleAccess(...)` where `handler` is typed as
    `IAuditEntityHandler` — no longer compiles.
  - A **direct call on the shipped concrete handler** — `handler.HandleAccess(...)` where `handler` is typed
    as `AuditEntityHandler` — also no longer compiles (`CS1061`). The public method was removed from the class
    as well as from the interface.

  **Behavioural change, only if you wrote your own handler.** With the shipped `AuditEntityHandler` no
  *runtime behaviour* changes — its `HandleAccess` returned `false` and touched nothing, so no read was ever
  audited. (Compilation is a separate matter: see the concrete-caller row above.) But a
  *custom* handler whose `HandleAccess` did real work — stamping a property, logging, throwing — **stops being
  invoked on asynchronous reads**. If you relied on that, move the behaviour to the call site or to a
  `DbContext` interceptor; the repositories no longer offer a read hook.

  Changing the return type to `void` was considered and rejected: it removes the misleading signal but leaves
  the hazard, because a handler that stamps a property would still be invoked on async reads, still persist
  incidentally when the entity happens to be tracked, and still lose the stamp on `AsNoTracking` paths.

- **`ReadRepositoryAsync` no longer takes an `IAuditEntityHandler`** — with `HandleAccess` gone (#104), the
  async read repositories required a dependency they never called, and threw `ArgumentNullException` when it
  was not supplied. Both constructors now take only the `DbContext`, matching the synchronous
  `ReadRepository<TEntity>`. The `protected AuditEntityHandler` property (introduced by #99 within this
  same unreleased cycle — it never shipped; #104 only rewrote its documentation) is removed with it:
  nothing in the library read it, and
  `ReadWriteRepositoryAsync` keeps its own reference for `HandleCreation` / `HandleModification`.
  See `change-log/issue-111-drop-audit-handler-from-read-repository.md`. (#111)

  **Breaking change**, though narrower than it looks:

  | Who | Effect |
  |---|---|
  | Consumers resolving repositories through DI (`AddRepositories`, `AddDbContextWithRepositories`, `IUnitOfWork`) | **None** — the container simply satisfies the smaller constructor. `IAuditEntityHandler` remains registered for the write repositories. |
  | Subclasses of `ReadRepositoryAsync<TEntity>` / `ReadRepositoryAsync<TEntity, TId>` forwarding the handler to `base(...)` | **Source break** — `CS1729`, no two-argument constructor. Stop forwarding; inject your own handler if your subclass needs one. |
  | Direct `new ReadRepositoryAsync<...>(dbContext, handler)` calls | **Source break** — `CS1729`. Drop the second argument. |
  | Subclasses of `ReadWriteRepositoryAsync<TEntity, TId>` (the documented extension pattern) | **None** — its two-argument constructor is unchanged, and it still fails fast with `ArgumentNullException` on a `null` handler (the guard moved from the base class into `ReadWriteRepositoryAsync` itself). |
  | Compiled assemblies calling the removed two-argument constructor | **Binary break** — `MissingMethodException` until recompiled, which the major version already requires. |
  | Anyone reading the `protected AuditEntityHandler` property | **Source break** — but the property never appeared in a released version, so this can only affect builds against the unreleased 4.0 branch. |

### Changed

- **`IReadRepository<TEntity>.FindFirst` no longer takes a `CancellationToken`** — the parameter was
  declared but silently ignored: the synchronous `DbSet.FirstOrDefault` has no overload that accepts a
  token, so a caller passing one got no cancellation and no error. It was also the only synchronous
  method on the interface to advertise cancellation — `GetAll`, `GetPage`, `Count` and `GetById` never
  did. Advertising cooperative cancellation that cannot be delivered is worse than not advertising it,
  so the parameter is gone rather than partially honoured. `FindFirstAsync` is unaffected and still
  takes a token, which it genuinely uses.
  See `change-log/issue-102-remove-unused-cancellationtoken-from-findfirst.md`. (#102)

  **Breaking change**, and it lands differently for implementors than for callers:

  | Who | Source | Binary |
  |---|---|---|
  | Anyone **implementing** `IReadRepository<TEntity>` (or `IReadWriteRepository<TEntity, TId>`) directly | **Breaks** — `CS0535`, whether or not the token was ever used | Breaks |
  | Callers writing `FindFirst(predicate)` or `FindFirst(predicate, onDbSet)` | Compiles unchanged | **Breaks until recompiled** — optional arguments are baked in at the call site, so existing IL still calls the three-parameter method |
  | Callers passing the token (positionally or as `cancellationToken:`) | **Breaks** — `CS1501` / `CS1739` | Breaks |
  | Types deriving from `ReadRepository<TEntity>` / `ReadWriteRepository<TEntity, TId>` without redeclaring the method | Compiles unchanged | Unaffected — no `FindFirst` method is emitted in the derived assembly; it inherits the updated base implementation |

  Unlike the `GetPage` change below, **nothing rebinds silently**: no call site keeps compiling while
  quietly changing meaning. Source breaks — implementors, and callers that pass a token — fail the build
  outright. The rest is binary-only: existing compiled callers of the one- and two-argument forms keep
  compiling from source and simply need rebuilding against 4.0, which a major version already requires.
  Implementors must delete the parameter from their own signature. Test doubles and mock setups written
  against the three-parameter form
  (`Setup(r => r.FindFirst(It.IsAny<...>(), It.IsAny<...>(), It.IsAny<CancellationToken>()))`)
  also need updating.

- **Synchronous read repositories gained the `query` and `sortBy` parameters their async
  counterparts already had** — `IReadRepository<TEntity>.GetAll` and `.Count` offered no way to
  filter, leaving `onDbSet` (intended for eager loading and other query *shaping*) as the only
  lever, and `.GetPage` was missing the `sortBy` that `GetPageAsync` provides. All three now match
  the asynchronous signatures. XML documentation on every method carrying `onDbSet` now states the
  contract explicitly: shape the query with it, never filter.
  See `change-log/issue-77-query-parameter-on-sync-read-methods.md`. (#77)

  **Breaking change:** the new parameters shift positions on the synchronous interface.

  - `GetAll(q => q.Include(...))` no longer compiles; use `GetAll(onDbSet: q => q.Include(...))`.
  - `GetPage(1, 20, predicate, q => q.Include(...))` — four positional arguments ending in a shaping
    lambda — no longer compiles; name the arguments. Note that *not every* positional `GetPage` call
    fails loudly; see the warning below.
  - Custom `IReadRepository<TEntity>` implementations must update `GetAll`, `GetPage`, and `Count`.
    This is also a binary break — `Count()` and `Count(Expression<Func<TEntity, bool>>?)` are
    different metadata methods even though source calls to `Count()` still compile.

  **⚠️ Some `GetPage` calls change meaning without a compiler error.** `sortBy` is
  `Expression<Func<TEntity, object>>`, which a `bool`-bodied lambda converts to implicitly (the
  `bool` boxes), so a pre-4.0 predicate in third position now binds to `sortBy`:

  ```csharp
  repository.GetPage(1, 20, post => post.IsPublished);
  // before: filtered to published posts
  // now:    orders by a boxed bool and returns an UNFILTERED page
  //
  // fix:    repository.GetPage(1, 20, query: post => post.IsPublished);
  ```

  **Audit by the third argument, not the argument count.** Any `GetPage` call whose third argument
  is positional *and is a lambda* rebinds silently, however many arguments follow and whether or not
  they are named — including `GetPage(1, 20, pred, null)` and
  `GetPage(1, 20, pred, onDbSet: q => q.Include(...))`, the latter being the style this library's own
  docs and tests used before 4.0. Passing a typed `Expression<Func<T, bool>>` variable in third
  position fails to compile instead (`Expression<T>` is invariant), as does a fourth positional
  shaping lambda. `GetAll` and `Count` have no equivalent hazard — every positional `GetAll`
  migration is a hard compile error. Full detail and the safe/unsafe shape list are in
  `change-log/issue-77-query-parameter-on-sync-read-methods.md`.

### Fixed

- **Repository updates no longer blank creation-audit properties on partial detached updates** —
  `ReadWriteRepositoryAsync<TEntity, TId>.UpdateAsync` and `ReadWriteRepository<TEntity, TId>.Update`
  used `CurrentValues.SetValues`, which copies every scalar from the supplied entity. Passing a
  partial detached entity (e.g. `new Entity { Id = id, Name = "x" }`) silently overwrote the
  persisted `CreatedTime` and `CreatedBy` values with defaults. The persisted values are now
  restored after the copy, and the properties are excluded from the update.
  See `change-log/issue-88-preserve-creation-audit-on-update.md`. (#88)

  **Behavioural change:** when auditing is enabled (`RepositoriesConfiguration.EnableAuditing`,
  the default), creation-audit properties (`IHasCreatedTime.CreatedTime`,
  `IHasCreatedBy.CreatedBy`) are now write-once through the repository — values supplied to
  `Update`/`UpdateAsync` are ignored and the persisted values are kept. To deliberately amend
  creation-audit data (e.g. backfilling imported rows), use the `DbContext` directly or disable
  auditing. With auditing disabled, updates keep plain full-entity semantics. All other properties
  keep full-entity update semantics: any property left unset on the supplied entity is still
  written to the store with its default value.

  **Breaking change:** `IAuditEntityHandler` gained a new `IsAuditingEnabled` property. Custom
  implementations of the interface must implement it.

### Security

- **Fixed vulnerable transitive `SQLitePCLRaw.lib.e_sqlite3` (high severity,
  [GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q))** — `Ploch.Data.EFCore.SqLite`
  now references `SQLitePCLRaw.bundle_e_sqlite3` 3.0.3 directly, overriding the vulnerable 2.1.x
  version that EF Core still pulls transitively. All packages depending on the SQLite provider
  resolve the patched native SQLite library. See `change-log/issue-91-vulnerable-sqlitepclraw.md`. (#91)

### Documented

- **The nullability contract of the model properties is now stated explicitly** — `INamed.Name`,
  `INamedReadOnly.Name`, `IHasTitle.Title`, `IHasTitleReadOnly.Title`, `IHasId<TId>.Id`,
  `IGetOnlyId<TId>.Id`, `IHasValue<TValue>.Value` and `IHasTags<TTag, TTagId>.Tags` are annotated as
  non-nullable, but the common types supplied in `Ploch.Data.Model` (`Property<TId, TValue>`,
  `Tag<TId>`, `Category<TCategory, TId>`, `Image`) do not assign them at construction — a
  reference-type or open generic property uses a null-forgiving initialiser (`= null!` or
  `= default!`), while a closed value-type property such as `Image.Id` reaches the same state
  implicitly. The same contract is now documented in Markdown as well: `docs/data-model.md` gains a
  **Nullability contract** section referenced from the Interface Reference table, and the packaged
  `Ploch.Data.Model` README explains the `= null!` in its Quick Start. A freshly constructed entity therefore
  carries `null` (or `default(T)`) until the caller assigns a value or Entity Framework Core
  materialises the entity, and a deliberate null-forgiving assignment can set the property back to
  `null`. The null-forgiving initialiser exists so that the compiler accepts the EF Core
  materialisation path, on which the ORM populates the property after construction. Validation
  metadata is a separate concern from assignment behaviour: `Tag<TId>.Name` carries `[Required]`,
  which constrains validation and the generated column rather than in-memory assignment.

  The remarks describe the **supplied common types**, not a guarantee the interfaces impose on their
  implementers: an implementation outside this library is free to be stricter, and the repository's
  own test model does exactly that (`Blog.Name` is declared `required`).

  This is a **documentation-only** change: the runtime behaviour and the public API signatures are
  unchanged for v4.0. Making the properties `required` was rejected because it breaks construction
  without an object initialiser; annotating them as `string?` was rejected because it pushes null
  checks onto every consumer and weakens the model interfaces that exist to standardise these
  property shapes; and a runtime guard on the setter was rejected because entities in this workspace
  are plain data carriers with no business logic. Assigning a value before the entity is used or
  persisted remains the caller's responsibility. (#131)

## v2.1 — NBGV Versioning and Release Pipeline

### Overview

This release introduces automated versioning via Nerdbank.GitVersioning
(NBGV) and a fully automated release pipeline for publishing packages
to NuGet.org.

### What's New

#### Automated Versioning (Nerdbank.GitVersioning)

- Replaced manual `VersionPrefix`/`RELEASEVERSION` env var approach with NBGV
- Version is now derived from `version.json` and git commit height
- Development builds produce prerelease packages (e.g., `2.1.5-prerelease`)
- Release builds produce stable packages (e.g., `3.0.0`)

#### Release Pipeline

- New GitHub Actions workflow (`release.yml`) for one-click releases
- Accepts a version number, builds, tests, and publishes to NuGet.org
- Automatically creates git tags and GitHub Releases with release notes
- Bumps the version for the next development cycle after release

#### Open-Source Publishing Enhancements

- Packages are now published to **NuGet.org** for releases
- **SourceLink** enabled — consumers can step into library source code during debugging
- **Symbol packages** (`.snupkg`) published to the NuGet symbol server
- **Deterministic builds** enabled in CI for reproducible packages
- Development/PR packages continue to publish to GitHub Packages

### Migration Notes

- The `RELEASEVERSION` environment variable is no longer used
- Version is controlled via `version.json` at the repository root
- Use the `nbgv` dotnet tool (`dotnet tool restore && dotnet nbgv get-version`) to inspect the current version locally
