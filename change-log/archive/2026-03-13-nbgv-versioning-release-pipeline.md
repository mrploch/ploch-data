## NBGV Versioning and Release Pipeline

### Automated Versioning

- Switched from manual `VersionPrefix`/`RELEASEVERSION` environment variable to **Nerdbank.GitVersioning (NBGV)**
- Version is now derived from `version.json` at the repository root and git commit height
- Development builds produce `-prerelease` suffixed versions automatically
- Public releases use clean semver versions (e.g., `3.0.0`)

### Release Pipeline

- Added `release.yml` workflow (manually triggered via `workflow_dispatch`)
- Accepts a version number, builds in Release configuration, runs all tests
- Publishes `.nupkg` and `.snupkg` packages to **NuGet.org**
- Creates git tags and GitHub Releases with release notes from `change-log/`
- Automatically bumps to the next development prerelease version

### Open-Source Publishing

- Enabled **SourceLink** for source-level debugging by package consumers
- Enabled **symbol packages** (`.snupkg`) for the NuGet symbol server
- Enabled **deterministic builds** in CI for reproducible packages
- CI continues to publish prerelease packages to GitHub Packages

### CI Improvements

- Added `dotnet tool restore` step for NBGV tool
- Passed secrets via environment variables instead of inline expansion (SonarCloud security rule)
- Added `.snupkg` publishing to GitHub Packages

### Breaking Changes

- The `RELEASEVERSION` environment variable is no longer recognised
- Version is now controlled exclusively via `version.json` using Nerdbank.GitVersioning
