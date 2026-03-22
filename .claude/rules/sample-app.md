# Sample Application Rules

The `samples/SampleApp/` directory contains a **Knowledge Base sample application** that demonstrates how an **external consumer** would use the Ploch.Data libraries (GenericRepository, Unit of Work, EF Core utilities, etc.) from published NuGet packages.

## Critical Constraints

### PackageReference only — never ProjectReference for Ploch.Data packages

The SampleApp **must** use `PackageReference` for all Ploch.Data packages (`Ploch.Data.Model`, `Ploch.Data.GenericRepository.EFCore`, `Ploch.Data.EFCore`, `Ploch.Data.EFCore.SqLite`, `Ploch.Data.EFCore.SqlServer`, `Ploch.Data.GenericRepository.EFCore.IntegrationTesting`). **Never** convert these to `ProjectReference`.

The whole point of this sample is to validate the developer experience of consuming Ploch.Data as NuGet packages — the same way any external developer would. Using `ProjectReference` defeats this purpose entirely.

### Standalone build configuration — no parent imports

The SampleApp has its own `Directory.Build.props` and `Directory.Packages.props` that are **self-contained**. They must **not** import or inherit from the parent repo's build configuration files. An external consumer would not have access to `mrploch-development/dependencies/` or the repo's root `Directory.Build.props`.

If the SampleApp's build files import from parent directories, it is no longer representative of an external consumer project.

### Not part of the main solution

The SampleApp is **excluded** from `Ploch.Data.slnx` and the CI pipeline (`build-dotnet.yml`). It has its own solution file: `samples/SampleApp/Ploch.Data.SampleApp.slnx`.

**Why:** The SampleApp depends on published NuGet packages that are not available during the CI build of the library itself. Including it in the main solution causes NuGet restore failures (NU1010, NU1301).

### Package versions are managed independently

The SampleApp's `Directory.Packages.props` defines its own `PlochDataPackagesVersion` variable and all package versions explicitly. When a new version of the Ploch.Data packages is published, this version must be updated manually.

## What this means in practice

- **Do not** add SampleApp projects to `Ploch.Data.slnx`.
- **Do not** replace `PackageReference` with `ProjectReference` for Ploch.Data packages, even to "fix" build errors.
- **Do not** add `<Import>` directives that reference files outside `samples/SampleApp/`.
- **Do** treat the SampleApp as if it were a completely separate repository that happens to live inside this one for documentation purposes.
- **Do** update the `PlochDataPackagesVersion` in `samples/SampleApp/Directory.Packages.props` after publishing new package versions.
