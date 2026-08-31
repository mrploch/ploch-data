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

A second, harder failure of the same kind sat in the repository's own `NuGet.Config`. It declared a
machine-specific folder feed:

```xml
<add key="local" value="C:\DevNet\my\mrploch\local-nuget-feed" />
```

mapped to the `Ploch.*` package pattern. Every restore that has to look a `Ploch.*` package up in a feed
therefore queries a path that exists on exactly one developer's machine, and NuGet reports a missing
folder source as `NU1301` — a hard restore error, not a warning:

```
error NU1301: The local source '.../C:\DevNet\my\mrploch\local-nuget-feed' doesn't exist.
```

This stayed dormant only because CI builds the sample with `-p:UsePlochProjectReferences=true`, which
strips every `Ploch.Data` `PackageReference`. Adding `Ploch.CommandLine.Spectre` (issue #101) introduced
the first `Ploch.*` package reference that survives that switch, so CI restore began failing for
`Ploch.Data.SampleApp.ConsoleApp` and, transitively, `Ploch.Data.SampleApp.IntegrationTests`.

# Change

- Bumped `PlochDataPackagesVersion` from `3.1.6-prerelease` to `3.1.39-prerelease`, whose transitive
  `Ploch.Common` constraints all resolve exactly.
- Removed the machine-specific `local` folder feed and its package-source mapping from the repository's
  `NuGet.Config`, which now matches the equivalent file in `ploch-commandline`: `nuget.org` for public
  packages, GitHub Packages for `Ploch.*`. A developer who wants a local folder feed adds it to their
  user-level `NuGet.Config`, where it cannot break anyone else's restore. A comment in the file records
  why.

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

The `NU1301` failure was reproduced locally by pointing the `local` source at a non-existent directory and
running the restore exactly as CI does:

```
dotnet restore ./Ploch.Data.slnx -p:UsePlochProjectReferences=true --configfile <repro config>   --packages <throwaway directory>
```

which failed on the same two projects as the CI run. With the `local` source removed, the same command
against a cold packages directory restores every project with zero warnings and zero errors.

`NU1603` is therefore **not** added to `NoWarn`. The suppression the issue offered as a fallback is
unnecessary, and adding it would hide the next occurrence of the same problem.

# Impact

SampleApp only. No published Ploch.Data package changes behaviour, and no public API changed.
