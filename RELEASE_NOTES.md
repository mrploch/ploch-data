# Release Notes

## Unreleased

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
