# Release Notes

## Unreleased

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
