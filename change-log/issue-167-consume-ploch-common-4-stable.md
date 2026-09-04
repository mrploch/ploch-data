## Dependencies: consume the stable Ploch.Common 4.0 release

### Fixed

- **Stable `Ploch.*` releases on nuget.org were invisible to this repository.** `NuGet.Config`
  mapped the `Ploch.*` pattern to the GitHub Packages feed only. NuGet source mapping resolves by
  the *most specific* matching pattern, so the `*` entry under `nuget.org` never applied to a Ploch
  package and nuget.org was never consulted for one. Since stable releases go to nuget.org and only
  prereleases go to GitHub Packages, every published stable Ploch package was unreachable:

  ```text
  error NU1103: Unable to find a stable package Ploch.Common with version (>= 4.0.47)
    - Found 603 version(s) in github [ Nearest version: 4.0.48-pr.328...gc5389e66e6 ]
    - Versions from nuget.org were not considered
  ```

  The pattern is now listed under both sources, so NuGet searches both. This is the precondition
  for consuming the stable `Ploch.Common` 4.0 release — the first gate item of the 4.0.0 release
  (#94).

### Changed

- **`Ploch.Common` and friends move from 3.0.0 to 4.0.47.** The version numbers themselves live in
  the shared `mrploch-development/dependencies/Ploch.Packages.props`, which this repository imports,
  so the bump is **a change in another repository** — mrploch/mrploch-development#15, tracking
  mrploch/mrploch-development#14. Nothing in ploch-data declares those versions, which is why no
  `.props` file changes here.

  That file drove both package families from a single `PlochPackagesVersion`, which stopped being
  expressible once ploch-common released 4.0.47 while ploch-data remained on 3.x. It is now split
  into `PlochCommonPackagesVersion` and `PlochDataPackagesVersion`.

  Verified by running the release pack path (`dotnet pack -p:UseProjectReferences=false`) and
  reading the resulting nuspecs. Before:

  ```text
  Ploch.Data.GenericRepository        -> Ploch.Common 3.0.0, Ploch.Common.AppServices 3.0.0
  Ploch.Data.GenericRepository.EFCore -> Ploch.Common.AppServices 3.0.0, Ploch.Common.DependencyInjection 3.0.0
  ```

  After:

  ```text
  Ploch.Data.GenericRepository        -> Ploch.Common 4.0.47, Ploch.Common.AppServices 4.0.47
  Ploch.Data.GenericRepository.EFCore -> Ploch.Common.AppServices 4.0.47, Ploch.Common.DependencyInjection 4.0.47
  ```

  Without this, `Ploch.Data` 4.0.0 would have shipped declaring a dependency on `Ploch.Common`
  **3.0.0** — three majors behind, and missing every fix in the 4.0 release.
