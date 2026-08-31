# Context

`UnitOfWorkRepositoryAsyncSQLiteInMemoryTests` failed on every local `dotnet test -c Release` run with:

```
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
- Packages produced by a Release build now declare a dependency on the `ploch-common` checkout's
  version (currently `4.0.x-prerelease`) rather than the `3.0.0` central version. Those workflows
  clone `ploch-common` at its default branch, so pinning the clone to a released tag before the
  next nuget.org release is worth doing separately.

Refs: #95
