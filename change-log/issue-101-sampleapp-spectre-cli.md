# Context

The SampleApp is the executable documentation for the Ploch.Data libraries, so it should demonstrate the
conventions the rest of the organisation follows. It did not: the whole sample ran as a single
top-level-statement script in `Program.cs`. It could be watched, but not driven — there was no way to
exercise one feature in isolation — and it did not use `Ploch.CommandLine.Spectre`, the organisation's CLI
framework.

# Change

- Reworked the console app onto `Ploch.CommandLine.Spectre` `1.0.4-prerelease`. `Program.cs` is now an
  `AppBuilder.Create(args).ConfigureServices(...).ConfigureCommandApp(...).RunAsync(args)` host, which wires
  `Microsoft.Extensions.Hosting` configuration and dependency injection into a Spectre.Console.Cli command
  app.
- Split each operation the sample used to perform in sequence into its own command class with its own
  options: `demo`, `seed`, `list`, `show`, `search`, `update`, `authors`. Every capability the old script
  demonstrated is preserved — audit timestamps, hierarchical categories, tags, entity properties, eager
  loading through `onDbSet`, filtered queries, pagination, unit-of-work commits, and direct repository
  injection — and `demo` still runs the whole tour in one go.
- Added `SampleDataSeeder`, the seeding logic shared by `seed` and `demo`, and `SampleAppCommand<TSettings>`,
  a base class that opens a dependency-injection scope per command so the scoped `DbContext` and
  repositories are never resolved from the root container. That base class also exposes a settings-only
  `ExecuteAsync` overload, which is the seam the end-to-end tests drive.
- Added `SampleAppCommandsTests`: end-to-end tests covering every command's success path, its not-found
  path, and its settings validation.
- Changed the connection string to `DataSource=sampleapp.db`, so the database is created beside the app and
  the sample can be run without first generating migrations. Added a `.gitignore` for the resulting file.
- Rewrote `samples/SampleApp/README.md` around a command reference and pointed it at the
  `Ploch.CommandLine.Spectre` and Spectre.Console.Cli documentation.

# Impact

SampleApp only. No published Ploch.Data package changes behaviour, and no public API changed.
