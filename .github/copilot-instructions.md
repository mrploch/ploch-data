# GitHub Copilot Instructions — Ploch.Data

## Sample Application Rules

The `samples/SampleApp/` directory contains a Knowledge Base sample application that demonstrates how an **external consumer** would use the Ploch.Data libraries from published NuGet packages.

### Critical constraints

- **PackageReference only** — The SampleApp must use `PackageReference` for all Ploch.Data packages. Never convert these to `ProjectReference`. The sample validates the developer experience of consuming Ploch.Data as NuGet packages.
- **Standalone build configuration** — The SampleApp's `Directory.Build.props` and `Directory.Packages.props` are self-contained. They must not import from parent directories. An external consumer would not have access to `mrploch-development/dependencies/` or the repo root build files.
- **Not part of the main solution** — The SampleApp is excluded from `Ploch.Data.slnx` and CI. It has its own solution: `samples/SampleApp/Ploch.Data.SampleApp.slnx`. Including it in the main solution causes NuGet restore failures because the Ploch.Data packages are not published at build time.
- **Independent package versions** — The SampleApp defines its own `PlochDataPackagesVersion` in `Directory.Packages.props`. Update this after publishing new Ploch.Data package versions.

### Do not

- Add SampleApp projects to `Ploch.Data.slnx`.
- Replace `PackageReference` with `ProjectReference` for Ploch.Data packages.
- Add `<Import>` directives referencing files outside `samples/SampleApp/`.

### Do

- Treat the SampleApp as a separate repository that happens to live inside this one.
- Update `PlochDataPackagesVersion` after publishing new package versions.
