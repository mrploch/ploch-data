# GitHub Copilot Instructions — Ploch.Data

## Sample Application Rules

The `samples/SampleApp/` directory contains a Knowledge Base sample application that demonstrates how an **external consumer** would use the Ploch.Data libraries from published NuGet packages. It supports two build modes:

- **Standalone**: `dotnet build Ploch.Data.SampleApp.slnx` — uses PackageReference (external consumer experience)
- **Solution mode**: `dotnet build Ploch.Data.slnx -p:UsePlochProjectReferences=true` — automatically switches to ProjectReference via `ProjectReferences.props` to catch breaking changes

### Critical constraints

- **Never manually edit csproj files to swap references** — The switching is automatic via `ProjectReferences.props`. csproj files must only contain `PackageReference` for Ploch.Data packages.
- **Standalone build configuration** — The SampleApp's `Directory.Build.props` and `Directory.Packages.props` are self-contained. They must not import from parent directories.
- **Independent package versions** — The SampleApp defines its own `PlochDataPackagesVersion` in `Directory.Packages.props`. Update this after publishing new Ploch.Data package versions.
- **Update ProjectReferences.props** when adding new Ploch.Data library packages.

### Do not

- Replace `PackageReference` with `ProjectReference` in csproj files (the switch is automatic).
- Add `<Import>` directives referencing files outside `samples/SampleApp/` (except the conditional `ProjectReferences.props` import in `Directory.Build.props`).

### Do

- Treat SampleApp csproj files as if they were in a separate repository.
- Update `PlochDataPackagesVersion` after publishing new package versions.
