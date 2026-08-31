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

# Impact

- No breaking API changes. No existing test passed `useScopedProvider: false`, so no existing
  behaviour moves.
- New tests: `TestDbContextHarnessTests` (7) covering construction, deconstruction, idempotent
  synchronous and asynchronous disposal, and the legacy tuple path;
  `RepositoryScopeLifetimeTests` (5) pinning the scoped-versus-fresh-scope resolution semantics.

Refs: #80
