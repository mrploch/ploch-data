---
apply: always
---

# Sample Application Rules

The `samples/SampleApp/` directory contains a **Knowledge Base sample application** that demonstrates how an **external consumer** would use the Ploch.Data libraries (GenericRepository, Unit of Work, EF Core utilities, etc.) from published NuGet packages.

## Dual-Mode Build

The SampleApp supports two build modes:

### Standalone mode (default)

```bash
cd samples/SampleApp
dotnet build Ploch.Data.SampleApp.slnx
```

Uses `PackageReference` for Ploch.Data packages — exactly as an external consumer would. Requires the packages to be published on the NuGet feed.

### Solution mode (CI / PR validation)

```bash
dotnet build Ploch.Data.slnx -p:UsePlochProjectReferences=true
```

The `ProjectReferences.props` file automatically replaces Ploch.Data `PackageReference` items with `ProjectReference` items pointing to the library source code. This catches breaking changes at PR time.

## How the switching works

1. Each csproj file contains only `PackageReference` for Ploch.Data packages (the external consumer view)
2. `samples/SampleApp/Directory.Build.props` conditionally imports `ProjectReferences.props` when `UsePlochProjectReferences=true`
3. `ProjectReferences.props` removes all Ploch.Data PackageReferences and adds ProjectReferences to the corresponding source projects
4. The CI workflow passes `-p:UsePlochProjectReferences=true` on all dotnet commands

## Critical Constraints

### Never manually edit csproj files to swap references

The PackageReference ↔ ProjectReference switching is handled **automatically** by `ProjectReferences.props`. Never manually convert PackageReferences to ProjectReferences (or vice versa) in any SampleApp csproj file.

### Standalone build configuration — no parent imports

The SampleApp has its own `Directory.Build.props` and `Directory.Packages.props` that are **self-contained**. They must **not** import or inherit from the parent repo's build configuration files. An external consumer would not have access to `mrploch-development/dependencies/` or the repo's root `Directory.Build.props`.

### Package versions are managed independently

The SampleApp's `Directory.Packages.props` defines its own `PlochDataPackagesVersion` variable and all package versions explicitly. When a new version of the Ploch.Data packages is published, this version must be updated manually.

### SonarCloud

The SampleApp is analysed by SonarCloud for code issues but **excluded from coverage** metrics (`sonar.coverage.exclusions` includes `**/samples/**`).

## What this means in practice

- **Do not** replace `PackageReference` with `ProjectReference` for Ploch.Data packages in csproj files.
- **Do not** add `<Import>` directives that reference files outside `samples/SampleApp/` (except `ProjectReferences.props` which is conditionally imported).
- **Do** treat the SampleApp csproj files as if they were in a completely separate repository.
- **Do** update `PlochDataPackagesVersion` in `samples/SampleApp/Directory.Packages.props` after publishing new package versions.
- **Do** update `ProjectReferences.props` if new Ploch.Data packages are added to the library.
