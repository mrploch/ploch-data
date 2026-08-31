# Context

The SampleApp consumes the Ploch.Data libraries the way an external consumer does — through
`PackageReference`, never `ProjectReference` — so it is the only thing in this repository that proves the
published packages restore on a machine that has never built ploch-data. It did not.

`samples/SampleApp/Directory.Packages.props` pinned `PlochDataPackagesVersion` to `3.1.6-prerelease`. That
version resolves, but its transitive constraints name NBGV prerelease versions carrying commit hashes that
are no longer on the feed, so every restore emitted `NU1603`:

```
warning NU1603: Ploch.Data.GenericRepository.EFCore 3.1.6-prerelease depends on
Ploch.Common.AppServices (>= 3.1.2-prerelease.g761d1230a3) but Ploch.Common.AppServices
3.1.2-prerelease.g761d1230a3 was not found. Ploch.Common.AppServices 3.1.2-prerelease.ga6bed07e9b
was resolved instead.
```

# Change

- Bumped `PlochDataPackagesVersion` from `3.1.6-prerelease` to `3.1.39-prerelease`, whose transitive
  `Ploch.Common` constraints all resolve exactly.

# Verification

A cold restore into a throwaway packages directory, so the machine's global package cache cannot mask a
missing package, and against a NuGet configuration carrying only `nuget.org` and GitHub Packages, so the
workspace's local folder feed cannot either:

```
dotnet restore samples/SampleApp/Ploch.Data.SampleApp.slnx \
  --configfile CleanMachine.NuGet.Config \
  --packages <throwaway directory>
```

completes with zero warnings. Every `Ploch.*` package in the resulting directory records
`"source": "https://nuget.pkg.github.com/mrploch/index.json"`. The same restore at `3.1.6-prerelease`
reproduces `NU1603` across five of the six projects.

`NU1603` is therefore **not** added to `NoWarn`. The suppression the issue offered as a fallback is
unnecessary, and adding it would hide the next occurrence of the same problem.

# Impact

SampleApp only. No published Ploch.Data package changes behaviour, and no public API changed.
