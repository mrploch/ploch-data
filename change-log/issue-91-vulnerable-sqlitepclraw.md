# Context

`SQLitePCLRaw.lib.e_sqlite3` versions `<= 2.1.11` carry a high-severity vulnerability in the bundled
native SQLite library ([GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q)).
Every Ploch.Data package that transitively pulls the SQLite provider (via
`Microsoft.EntityFrameworkCore.Sqlite`) shipped the vulnerable native library, because EF Core —
including the latest 10.0.x patch releases — still floors the dependency at the vulnerable `2.1.11`.

# Change

- Added a direct top-level `SQLitePCLRaw.bundle_e_sqlite3` **3.0.3** reference to
  `Ploch.Data.EFCore.SqLite`. The 3.x bundle ships the patched native SQLite
  (via `SourceGear.sqlite3` 3.50.4.5) and wins transitive version resolution, so all dependent
  projects and published packages now resolve the fixed native library.
- Central version added to `Directory.Packages.props` (and the SampleApp's standalone
  `Directory.Packages.props`).
- The SampleApp projects that consume published Ploch.Data packages carry the same direct
  override until they consume a package version that includes this fix.

# Impact

- `dotnet list package --vulnerable --include-transitive` no longer reports any vulnerable
  packages in Ploch.Data projects.
- No API changes. Consumers get `SQLitePCLRaw.bundle_e_sqlite3 >= 3.0.3` transitively from
  `Ploch.Data.EFCore.SqLite`.

Refs: #91
