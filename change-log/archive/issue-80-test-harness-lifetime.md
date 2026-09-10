# Context

Two lifetime/ownership defects in the integration-testing harness, split out of PR #75 review
feedback.

1. `DbContextServicesRegistrationHelper.BuildDbContextAndServiceProvider` returned a 3-tuple
   `(RootProvider, ScopedProvider, DbContext)`. Four resources need disposal — the root provider,
   the `IServiceScope` created internally, the shared `SqliteConnection`, and the `DbContext` — but
   the tuple conveys no ownership at all. `DataIntegrationTest` disposed them by hand; any other
   caller was on its own, and the internally created scope was not reachable to be disposed.

2. `GenericRepositoryDataIntegrationTest.GetServiceProvider(false)` returned the **root** provider.
   Repositories and `IUnitOfWork` are registered as scoped services, so resolving them from the root
   container promoted them to de-facto singletons and shared one `DbContext` (and its change
   tracker) across operations that were meant to be independent.

# Change

- Added `TestDbContextHarness<TDbContext>` (`Ploch.Data.EFCore.IntegrationTesting`) — a sealed
  `IDisposable`/`IAsyncDisposable` that owns the root provider, the initial scope, the shared
  connection and the initial `DbContext`, exposes all three references as properties, and supports
  tuple deconstruction.
- Added `DbContextServicesRegistrationHelper.BuildHarness<TDbContext>(...)` overloads returning it.
- `BuildDbContextAndServiceProvider` is unchanged in signature and behaviour but is now implemented
  on top of `BuildHarness`, so there is a single construction path.
- `DataIntegrationTest<TDbContext>` holds a harness and delegates disposal to it. Its
  `DbContext`, `ScopedServiceProvider` and `RootServiceProvider` members now throw
  `InvalidOperationException` — naming the initialisation requirement — if the harness is missing.
- Added `DataIntegrationTest<TDbContext>.CreateScope()`, which creates a scope from the root
  provider and tracks it for disposal with the test.
- `GetServiceProvider(false)` now resolves from a fresh tracked scope instead of the root provider.

The `bool useScopedProvider` parameter was **kept** rather than replaced with explicit
`CreateRepositoryInNewScope<T, TId>()` helpers: these types ship in published packages, and fixing
the parameter's semantics is not a breaking change while removing it would be.

Construction is now failure-safe: if `BuildServiceProvider()`, the scoped `TDbContext` resolution,
`OpenConnection()` or `EnsureCreated()` throws, everything already created is released — context,
scope, root provider, then the harness-owned connection — before the original exception propagates.
Cleanup failures are collected and dropped on purpose so they cannot mask the failure that aborted
construction.

Disposal is now failure-safe in the same way: a resource that throws no longer strands the resources
queued behind it, and the collected failures are rethrown afterwards (aggregated when more than one).
The root provider is released **before** the shared connection, so a singleton whose own `Dispose`
touches the database still observes an open connection.

`BuildHarness(IServiceCollection, string)` now also registers `IDbContextFactory<TDbContext>`, which
only the configurator overload did. Without it, a harness built from the connection-string overload
could not serve factory-based helpers such as `CreateRootDbContext()`.

`DataIntegrationTest<TDbContext>.CreateScope()` synchronises its scope list, so a test that fans out
concurrently cannot corrupt it, and the unbounded accumulation of scopes is documented on the method.

## Ownership: what the harness does *not* own

The harness owns the shared SQLite connection **only** on the connection-string overload, which is
the overload that creates it. On the `IDbContextConfigurator` overload — the one
`DataIntegrationTest` actually uses — the connection belongs to the configurator and the harness
receives `connection: null`, so the caller must dispose the configurator too.
`DataIntegrationTest.Dispose` does. The class remarks and `docs/integration-testing.md` previously
claimed unconditional ownership of all four resources; both now state the qualification.

## The tuple overloads deliberately do not expose the harness

Issue #80 asked for the tuple method to route through the wrapper "so the harness is reachable".
It routes through it; it does **not** return it, and that sub-requirement is consciously not
implemented: returning it would change the signature the overload exists to preserve.

The consequence is stated plainly rather than glossed. Cleanup is entirely the caller's job and takes
two disposals in order, because the root provider does **not** track the child scope it created —
disposing `RootProvider` alone leaves the scoped `TDbContext` undisposed. Callers must dispose
`ScopedProvider` first and `RootProvider` second, which is what
`TestDbContextHarnessTests.BuildDbContextAndServiceProvider_should_return_the_harness_references`
now does. That ordering is documented in the XML comments on both tuple overloads and in
`docs/integration-testing.md`, and it is precisely the footgun `BuildHarness` removes.

# Impact

- No breaking API changes: every signature is preserved, and replacing the get-only auto-properties
  with computed get-only properties is both source- and binary-compatible.
- **The behavioural contract of `useScopedProvider: false` does move**, for consumers of the
  published `Ploch.Data.GenericRepository.EFCore.IntegrationTesting` package even though no test in
  this repository passed `false`. A downstream test that resolved a repository with `false` used to
  get a root-cached instance sharing the test's `DbContext` and change tracker, and now gets an
  independent `DbContext` that cannot see tracked-but-unsaved entities.
  `RepositoryScopeLifetimeTests.UnscopedResolution_should_return_a_distinct_unit_of_work_for_every_call`
  pins exactly that instance-identity flip. This is called out in `RELEASE_NOTES.md` under
  "Changed behaviour (no API break)" so downstream repositories are warned.
- New tests: `TestDbContextHarnessTests` (11) covering construction, deconstruction, idempotent
  synchronous and asynchronous disposal, the legacy tuple path, the context-factory registration, the
  configurator overload's connection ownership, failure during preparation, and the
  `InvalidOperationException` initialisation guard; `RepositoryScopeLifetimeTests` (5) pinning the
  scoped-versus-fresh-scope resolution semantics.

Refs: #80
