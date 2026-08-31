# Context

`UnitOfWorkRepositoryAsyncSQLiteInMemoryTests` failed on every local `dotnet test -c Release` run with:

```text
System.IO.FileNotFoundException: Could not load file or assembly 'Ploch.Common, Version=4.0.31.43865'
   at Ploch.TestingSupport.XUnit3.AutoMoq.AutoDataCommonCustomization.Customize(IFixture fixture)
```

The test output directory mixed two generations of `Ploch.Common`:

| Assembly | Version | Source |
|---|---|---|
| `Ploch.TestingSupport.XUnit3.*` | 4.0.x | `ProjectReference` into the sibling `ploch-common` checkout |
| `Ploch.Common` and friends | 3.0.0 | NuGet package feed |

`Directory.Build.props` switched cross-repo references between `ProjectReference` (Debug) and
`PackageReference` (Release) via `UseProjectReferences`. That switch only ever applied to
`ploch-data`'s own projects. The test projects reference `Ploch.TestingSupport.XUnit3.AutoMoq` and
`Ploch.TestingSupport.XUnit3.Dependencies` **unconditionally** by project, because those two
projects are not published as NuGet packages — there is no `PackageVersion` for them anywhere in
the workspace. So in Release the test graph contained a `ProjectReference` to `Ploch.Common` (4.0.x,
resolved by NuGet at its placeholder `1.0.0`) *and* a `PackageReference` to `Ploch.Common` 3.0.0.
The package won restore, and the TestingSupport assemblies — compiled against 4.0.x — could not
bind at run time. .NET rolls an assembly reference *forward* to a higher version but never back to
a lower one, so the mismatch was fatal.

# Change

- `UseProjectReferences` now defaults to `true` in **every** configuration, not just Debug.
  `-p:UseProjectReferences=false` still forces `PackageReference` resolution for anyone who wants
  to verify the published packages' dependency shape.

Of the two options listed in the issue, only "all `ProjectReference`" is reachable today:
"all `PackageReference`" would require `Ploch.TestingSupport.XUnit3.AutoMoq` and
`Ploch.TestingSupport.XUnit3.Dependencies` to be published first, and they are not.

# Impact

- `dotnet build -c Release` and `dotnet test -c Release` are both green locally: 328 passed,
  1 skipped, 0 failed.
- CI is unaffected in kind — every workflow already clones `ploch-common` as a sibling before
  building, and `build-dotnet.yml` already built and published from a Debug (`ProjectReference`)
  build. `release.yml` and `deploy-nuget-org.yml` build **and test** in Release from one build, so
  they were subject to the same failure and are fixed by the same change.
- **Packaging is packed separately, not taken from the Release build.** A `ProjectReference`-resolved
  Release build emits packages whose nuspec declares the `ploch-common` checkout's version (currently
  `4.0.x-prerelease`) rather than the central `3.0.0`. No `Ploch.Common` 4.x exists on nuget.org, so
  publishing those would have shipped an unrestorable dependency graph — `release.yml` and
  `deploy-nuget-org.yml` both `dotnet build -c Release` and then push straight to
  `api.nuget.org`. Both workflows now run a dedicated
  `dotnet pack ./Ploch.Data.slnx -c Release -p:UseProjectReferences=false -o ./artifacts/packages`
  step after the tests and publish **only** from `./artifacts/packages`. Verified locally: the
  packed `Ploch.Data.GenericRepository.EFCore` nuspec declares
  `<dependency id="Ploch.Common.AppServices" version="3.0.0" />` for both `net10.0` and `net8.0`,
  where the build-produced nupkg declared `4.0.31-prerelease`.
- **The push is filtered to `Ploch.Data.*`.** Packing the solution also packs the sibling
  `ploch-common` projects reached through the unpublished `Ploch.TestingSupport.XUnit3.*` project
  references — the local pack produced `Ploch.Common`, `Ploch.Common.AppServices`,
  `Ploch.Common.DependencyInjection` and `Ploch.TestingSupport.FluentAssertions` alongside the 14
  `Ploch.Data.*` packages. Those belong to another repository. The previous `find . -name '*.nupkg'`
  never saw them only because `../ploch-common` sits outside the repository root; collecting
  everything into one output directory removes that accident, so the filter is explicit.
- **`ploch-common` is still cloned at its default branch**, not at a released tag
  (`release.yml:74`, `deploy-nuget-org.yml:26`, `build-dotnet.yml:32`, `publish-docs.yml:57`). That
  is tracked in **[#67](https://github.com/mrploch/ploch-data/issues/67)** and is no longer a release
  blocker now that packing is pinned to package resolution, but it still means the *tested* assembly
  set moves with ploch-common's default branch.
- **`build-dotnet.yml` is deliberately unchanged.** Its Release build publishes to GitHub Packages,
  where the `ploch-common` prereleases it depends on are actually available, so a
  `ProjectReference`-resolved dependency graph is restorable there.
- **Release now inherits Debug's `MSB3277`.** Making `ProjectReference` the Release default surfaces
  the pre-existing `Microsoft.Bcl.AsyncInterfaces` 8.0.0 vs 10.0.0.5 conflict on
  `Ploch.Data.GenericRepository.EFCore.SqlServer` in Release as well as Debug. It is not new to the
  repository — the same project at `-c Debug -p:UseProjectReferences=true` already emitted it, and at
  `-c Release -p:UseProjectReferences=false` emits none — but a solution-wide
  `dotnet build -c Release` is no longer warning-free. Aligning `Microsoft.Bcl.AsyncInterfaces`
  across the two repositories is the real fix and is out of scope here.

Refs: #95
