# Release Notes

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
