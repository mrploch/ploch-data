# GitHub Copilot Instructions — Ploch.Data

## Repository overview

This repository contains the Ploch.Data family of .NET packages for data models, EF Core helpers, provider-specific configuration, generic repositories, Unit of Work, and integration-testing support.

- Primary solution: `Ploch.Data.slnx`
- Standalone sample solution: `Ploch.Data.SampleApp.slnx`
- Key package families:
    - `Ploch.Data.Model`
    - `Ploch.Data.EFCore`, `Ploch.Data.EFCore.SqLite`, `Ploch.Data.EFCore.SqlServer`
    - `Ploch.Data.GenericRepository`, `Ploch.Data.GenericRepository.EFCore`, provider-specific variants, and specification support
    - integration-testing packages for EF Core and Generic Repository

## Build and test commands

- Restore: `dotnet restore`
- Build whole solution: `dotnet build Ploch.Data.slnx`
- Build whole solution with SampleApp switched to local project references: `dotnet build Ploch.Data.slnx -p:UsePlochProjectReferences=true`
- Build sample app in standalone consumer mode: `dotnet build Ploch.Data.SampleApp.slnx`
- Run all tests: `dotnet test`
- Run a specific test project: `dotnet test <path-to-csproj>`
- Run filtered tests: `dotnet test --filter "FullyQualifiedName~SomeTestName"`

## Quality bar

- Preserve the separation between provider-agnostic interfaces and EF Core implementations.
- Keep business-facing abstractions repository-provider agnostic where the design already intends that.
- Avoid architecture drift between the core packages, provider packages, and integration-testing packages.
- Prefer targeted changes over broad repository-wide refactors unless the task genuinely spans package boundaries.
- If shared abstractions or DI registration points change, validate downstream impact carefully.

## Sample Application Rules

The `samples/SampleApp/` directory contains a Knowledge Base sample application that demonstrates how an external consumer would use the Ploch.Data libraries from published NuGet packages. It supports two build modes:

- Standalone: `dotnet build Ploch.Data.SampleApp.slnx`
- Solution mode: `dotnet build Ploch.Data.slnx -p:UsePlochProjectReferences=true`

### Critical constraints

- Never manually edit csproj files to swap references. The switching is automatic via `ProjectReferences.props`. SampleApp csproj files must only contain `PackageReference` for Ploch.Data packages.
- The SampleApp `Directory.Build.props` and `Directory.Packages.props` are self-contained and must not import from parent directories.
- The SampleApp defines its own `PlochDataPackagesVersion` in `Directory.Packages.props`. Update that after publishing new Ploch.Data package versions.
- Update `ProjectReferences.props` when adding new Ploch.Data library packages.

### Do not

- Replace `PackageReference` with `ProjectReference` in SampleApp csproj files.
- Add `<Import>` directives referencing files outside `samples/SampleApp/` except the existing conditional `ProjectReferences.props` import in `Directory.Build.props`.

### Do

- Treat SampleApp csproj files as if they were in a separate repository.
- Validate both normal solution behavior and SampleApp behavior when a change can affect external consumers.

## Testing conventions

- Use xUnit and FluentAssertions.
- Prefer `[Theory]` whenever practical.
- Keep test names in the style `MethodName_should_explain_what_it_should_do()`.
- Favor both repository-level tests and integration tests when behavior crosses EF Core, repositories, or DI registration.

## Documentation

- Use XML documentation comments for all public methods. Try to provide examples where it makes sense.
- Always keep the documentation markdown files in `docs` folder in the repository root [docs/](../docs/) up to date. If new features are being added, then those docs need to be extended to include the new feature usage documentationo. If anything changes, then the docs need to be updated. Always provide examples in the docs when discussing a feature.

## Validation expectations

- Before finishing, run the most relevant tests for the changed projects.
- If a change affects shared repository abstractions, provider selection, or SampleApp packaging behavior, broaden validation beyond a single project.
- If you cannot run a needed validation step, say exactly what remains unverified.
