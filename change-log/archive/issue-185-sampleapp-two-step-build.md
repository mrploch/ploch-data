# Context

The published packages declared a dependency on one version of `Ploch.Common` while their
assemblies demanded another. Restore succeeded and the application failed the moment it first
touched the library:

```text
System.IO.FileNotFoundException: Could not load file or assembly 'Ploch.Common, Version=4.1.4.48466'
   at Ploch.Data.GenericRepository.EFCore.ServiceCollectionRegistration.AddRepositories<TDbContext>()
```

That is the same failure signature as [issue #95](https://github.com/mrploch/ploch-data/issues/95),
one layer out. #95 fixed it for the *test* graph; the packages themselves kept shipping it.

| | nuspec declares | assembly demands | consumer sees |
|---|---|---|---|
| before #95 | prerelease | prerelease | restore fails — the pinned prerelease is not on nuget.org |
| after #95 | `4.0.47` | `4.1.4.48466` | run-time `FileNotFoundException` |
| now | `4.0.47` | `4.0.47.23323` | works |

## Cause

`dotnet pack -p:UseProjectReferences=false` reaches only the **nuspec generator**. The compile has
already happened under the previous property value, and MSBuild's incremental check sees
up-to-date outputs, so it never recompiles. The property silently applied to half the operation:
the package described binaries it had not produced.

Nothing caught it because every existing check looked at a different artefact. Restore validated
the nuspec's dependency graph — consistent. Compile validated source against project references —
consistent. The tests ran against project-referenced binaries — consistent. Only executing the
packaged assembly against its own declared dependencies exposes the contradiction, and nothing did
that.

## Fixed

The release workflow now builds explicitly with `-p:UseProjectReferences=false --no-incremental`
and then packs with `--no-build`, so the assemblies and the nuspec are produced from the same
compilation. `--no-incremental` is not tidiness: the defect *was* an up-to-date check, so the skip
is disabled outright rather than relied upon to bust itself.

A **consumer gate** now stands between packing and publishing. The SampleApp — which resolves
`Ploch.Data.*` through `PackageReference`, exactly as an external project does — is built and
tested against a local feed of the artefacts about to be published. It gates locally rather than
against GitHub Packages because ids on a remote feed cannot practically be withdrawn: publishing
first and gating afterwards would strand an unusable version that could never be republished.

This gate is what found the defect above.

## For consumers

Packages from 4.0.0 onward declare dependency versions that match the assemblies inside them.
No action is required, and no API changed.

Anyone on `Ploch.Data.GenericRepository.EFCore` **3.0.1** should move to 4.0.0: that version pins
`Ploch.Common.AppServices 3.1.1-prerelease.g03bc6d62af`, a prerelease published only to GitHub
Packages, so it cannot be restored from nuget.org at all. Package content is immutable, so 3.0.1
cannot be repaired in place — see
[issue #187](https://github.com/mrploch/ploch-data/issues/187) for the reasoning behind leaving it
published rather than unlisting it.

Refs: [#185](https://github.com/mrploch/ploch-data/issues/185),
[#95](https://github.com/mrploch/ploch-data/issues/95),
[#187](https://github.com/mrploch/ploch-data/issues/187)
