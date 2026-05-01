# Ploch.Data Repository Research

This note captures a maintainer-focused map of the repository: packages, structure, build/test tooling, conventions, and external dependencies needed to work locally.

## Layout

- `src/` – library projects:
  - `Data.Model` – standard entity interfaces and base types (audit, hierarchical, tags/categories, properties).
  - `Data.EFCore` – EF Core utilities (connection string helper, `IDbContextConfigurator`, `BaseDbContextFactory`, data seeding).
  - `Data.EFCore.{SqLite,SqlServer}` – provider configurators and factories (SQLite also applies the `DateTimeOffset` converter fix).
  - `Data.GenericRepository` – provider-agnostic repository/UoW interfaces and audit handler abstractions.
  - `Data.GenericRepository.EFCore` – EF Core implementations plus DI registration; `.Specification` adds Ardalis.Specification helpers; provider-specific DI wrappers under `.EFCore.{SqLite,SqlServer}`; `.IntegrationTesting` has repository/UoW test base.
  - `Data.EFCore.IntegrationTesting` – EF Core integration test base (`DataIntegrationTest<TDbContext>`).
  - `Data.StandardDataSets`, `Data.Utilities` – datasets and utility helpers.
- `tests/` – integration and unit tests for EF Core, providers, GenericRepository, datasets.
- `docs/` – main documentation set (getting-started, architecture, data-model, generic-repository, dependency-injection, integration-testing, extending, data-project-setup).
- `samples/SampleApp/` – standalone consumer example with dual build modes (package vs project refs) showing modelling, DI, provider switching, migrations, and integration tests.
- `docfx_project/`, `DocumentationSite/` – doc generation/site output.
- Build scripts: `build.sh`/`build.ps1` (NUKE), `build-dotnet-commands.ps1`, `azure-pipelines.yml`, `qodana.yaml`, `docker-compose.yml`.

## Design Highlights

- Clear layering: `Model` (interfaces/POCOs) → `EFCore` utilities → provider packages → `GenericRepository` (interfaces) → `GenericRepository.EFCore` (impl/DI) → specification/integration testing helpers.
- DI entrypoints: `AddDbContextWithRepositories<TDbContext>()` (provider-specific packages) or `AddRepositories<TDbContext>()` for manual DbContext registration; lifecycle plugin via `IDbContextCreationLifecycle` injected into DbContext.
- Repository interfaces emphasise interface segregation (`IReadRepositoryAsync`, `IReadWriteRepositoryAsync`, `IQueryableRepository`, `IUnitOfWork`); audit handling via `IAuditEntityHandler` + `IUserInfoProvider`.
- Testing support: `DataIntegrationTest<TDbContext>` (in-memory SQLite) and `GenericRepositoryDataIntegrationTest<TDbContext>` with helpers; sample app tests demonstrate repository and UoW behaviours.

## Build, Test, and Tooling

- Target frameworks: libraries default to `net10.0` (some multi-target `net8.0`); nullable enabled; analyzers on with `TreatWarningsAsErrors` for test projects.
- Versioning/packaging: Nerdbank.GitVersioning (`version.json`), SourceLink, central package management (`Directory.Packages.props`).
- Builds/tests typically via `dotnet test Ploch.Data.slnx` or NUKE (`./build.sh`), but the solution uses conditional project references to a sibling **ploch-common** repo and shared props under `../mrploch-development/dependencies`. Without those checkouts, Debug/default builds fail with missing project/props files. Use `-p:UseProjectReferences=false` or Release with packaged dependencies when the shared repos are unavailable (packages must be in the feed).
- Baseline in this workspace: `dotnet test Ploch.Data.slnx` currently fails because `../ploch-common` projects are absent.

## Sample Application Rules (critical)

- Treat SampleApp as an external consumer: csproj files must keep `PackageReference` to Ploch.Data packages; switching to `ProjectReference` happens automatically via `ProjectReferences.props` (do not edit csproj to swap).
- SampleApp `Directory.Build.props`/`Directory.Packages.props` are self-contained; do not import from parents.
- Update `ProjectReferences.props` when adding new Ploch.Data packages; bump `PlochDataPackagesVersion` in SampleApp package props after publishing new versions.

## Conventions to Note

- Default `UseProjectReferences=true` in non-Release builds (set in `Directory.Build.props`); makes local debugging depend on sibling repos.
- Tests use xUnit + FluentAssertions; coverlet included for coverage.
- Central connection-string helper (`ConnectionString.FromJsonFile`) expects `appsettings.json` copied to output.
- Auditing: entities implementing `IHasAuditProperties`/`IHasAuditTimeProperties` get timestamps set by `AuditEntityHandler`; `IUserInfoProvider` defaults to `NullUserInfoProvider`.

## Useful Commands

- Restore/build/tests (packages available, no sibling repos): `dotnet test Ploch.Data.slnx -p:UseProjectReferences=false`
- NUKE pipeline: `./build.sh` (uses `build/_build.csproj`)
- SampleApp: `dotnet build Ploch.Data.SampleApp.slnx` (standalone package mode) or `dotnet build Ploch.Data.slnx -p:UsePlochProjectReferences=true` (solution mode)
