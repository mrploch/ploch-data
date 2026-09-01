using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace Ploch.Data.EFCore.SqlServer.Tests;

/// <summary>
///     Starts a throw-away SQL Server container for the duration of the test class and
///     creates a dedicated database inside it.
/// </summary>
/// <remarks>
///     <para>
///         The container image, port and credentials are all managed by Testcontainers, so nothing
///         is hard-coded and no externally managed container has to be running beforehand.
///     </para>
///     <para>
///         SQL Server refuses a connection whose <c>Initial Catalog</c> does not exist yet, so the
///         fixture first connects to <c>master</c>, issues a <c>CREATE DATABASE</c> statement, and only
///         then hands out a connection string pointing at the newly created catalog.
///     </para>
///     <para>
///         When Docker is unavailable the fixture records a skip reason instead of failing, which lets
///         the tests skip gracefully on machines and CI agents without a Docker daemon.
///     </para>
/// </remarks>
public sealed class SqlServerContainerFixture : IAsyncLifetime
{
    /// <summary>
    ///     The name of the database created inside the test container.
    /// </summary>
    /// <remarks>
    ///     The container is thrown away after the test class, so a fixed name is unique per run in
    ///     practice; the <c>IF DB_ID(...) IS NULL</c> guard in the creation statement is belt-and-braces.
    /// </remarks>
    public const string DatabaseName = "PlochDataSqlServerTests";

    private const string ImageName = "mcr.microsoft.com/mssql/server:2022-latest";

    private MsSqlContainer? _container;

    /// <summary>
    ///     Gets the reason the SQL Server tests must be skipped, or <see langword="null" /> when the
    ///     container is available.
    /// </summary>
    public string? SkipReason { get; private set; }

    /// <summary>
    ///     Gets the connection string pointing at the database created inside the container.
    /// </summary>
    /// <remarks>Only meaningful when <see cref="SkipReason" /> is <see langword="null" />.</remarks>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets a value indicating whether the SQL Server container was started successfully.
    /// </summary>
    public bool IsAvailable => SkipReason is null;

    /// <summary>
    ///     Skips the calling test when the SQL Server container could not be started.
    /// </summary>
    public void SkipIfUnavailable()
    {
        Assert.SkipWhen(!IsAvailable, SkipReason ?? "The SQL Server container is unavailable.");
    }

    /// <summary>
    ///     Starts the container and creates the test database inside it.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialisation.</returns>
    public async ValueTask InitializeAsync()
    {
        if (TestcontainersSettings.OS.DockerEndpointAuthConfig is null)
        {
            SkipReason = "Docker is not available - no Docker endpoint could be detected.";

            return;
        }

        try
        {
            _container = new MsSqlBuilder(ImageName).Build();
            await _container.StartAsync();
        }
        catch (Exception exception) when (exception is DockerUnavailableException or DockerConfigurationException)
        {
            SkipReason = $"Docker is not available - {exception.Message}";

            return;
        }

        try
        {
            ConnectionString = await CreateDatabaseAsync(_container.GetConnectionString());
        }
        catch
        {
            // The container is already running at this point and xUnit does not reliably call
            // DisposeAsync on a fixture whose InitializeAsync threw, so it would otherwise leak.
            await _container.DisposeAsync();
            _container = null;

            throw;
        }
    }

    /// <summary>
    ///     Stops and removes the container.
    /// </summary>
    /// <returns>A task that represents the asynchronous clean-up.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    private static async Task<string> CreateDatabaseAsync(string masterConnectionString)
    {
        await using (var connection = new SqlConnection(masterConnectionString))
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();

            // The database name is a compile-time constant, so string interpolation carries no
            // injection risk here - and CREATE DATABASE does not accept a parameterised name.
            command.CommandText = $"IF DB_ID(N'{DatabaseName}') IS NULL CREATE DATABASE [{DatabaseName}];";
            await command.ExecuteNonQueryAsync();
        }

        var builder = new SqlConnectionStringBuilder(masterConnectionString) { InitialCatalog = DatabaseName };

        return builder.ToString();
    }
}
