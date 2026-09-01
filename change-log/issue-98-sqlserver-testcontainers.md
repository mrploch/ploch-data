# Context

`Ploch.Data.EFCore.SqlServer.Tests` had its only meaningful test disabled with
`[Fact(Skip = "SQL Server container connection is broken.")]`, and the test code hard-coded
`localhost,1401` with `sa` / `P@ssw0rd` against a container a developer (or a CI workflow) had to
start by hand. The SQL Server provider path therefore had no real integration coverage.

# Change

- SQL Server integration tests now run against a throw-away container managed by
  `Testcontainers.MsSql` (`SqlServerContainerFixture`, an xUnit v3 `IAsyncLifetime` class fixture).
  Image, port and credentials are all Testcontainers-managed - nothing is hard-coded.
- The fixture connects to `master` and issues `CREATE DATABASE` before pointing `Initial Catalog`
  at the test catalog, because SQL Server refuses a connection whose catalog does not exist and the
  test harness opens the connection eagerly.
- `DataContext_should_be_functional` is unskipped and executes for real; a second test asserts, via
  a server-side `SELECT DB_NAME()`, that the context is bound to the created catalog and not to
  `master`.
- The manual `Start SqlServer` / `Wait for SQL Server container` steps (and the `sa` password they
  carried) were removed from **all three** workflows that run the test suite: `build-dotnet.yml`,
  `release.yml` and `deploy-nuget-org.yml`.
- `Testcontainers.MsSql` 4.14.0 added to `Directory.Packages.props`.

# Impact

- **Docker is now required for SQL Server test coverage.** With a Docker daemon available the tests
  execute; without one the fixture records a skip reason and the tests report `[SKIP]` rather than
  failing.
- The manual `docker run ... -p 1401:1433` procedure and the repository `docker-compose.yml` are no
  longer used by the test suite.
- No API or packaging changes - this is a test-infrastructure change only.

Refs: #98
