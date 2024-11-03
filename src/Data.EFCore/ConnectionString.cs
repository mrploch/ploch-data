using System;
using Microsoft.Extensions.Configuration;

namespace Ploch.Data.EFCore;

/// <summary>
///     Provides functionality to retrieve connection strings from configuration files.
/// </summary>
public static class ConnectionString
{
    /// <summary>
    ///     Retrieves a connection string from a JSON configuration file.
    /// </summary>
    /// <param name="configurationFileName">The name of the configuration file to load. Defaults to "appsettings.json".</param>
    /// <param name="connectionStringName">The name of the connection string to retrieve. Defaults to "DefaultConnection".</param>
    /// <returns>A function that returns the connection string when invoked.</returns>
    public static Func<string?> FromJsonFile(string configurationFileName = "appsettings.json", string connectionStringName = "DefaultConnection")
    {
        return FromJsonFile(new ConfigurationBuilder(), configurationFileName, connectionStringName);
    }

    /// <summary>
    ///     Retrieves a connection string from a JSON configuration file using a specified ConfigurationBuilder.
    /// </summary>
    /// <param name="configurationBuilder">The ConfigurationBuilder used to build the configuration.</param>
    /// <param name="configurationFileName">The name of the configuration file to load. Defaults to "appsettings.json".</param>
    /// <param name="connectionStringName">The name of the connection string to retrieve. Defaults to "DefaultConnection".</param>
    /// <returns>A function that returns the connection string when invoked.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static Func<string?> FromJsonFile(ConfigurationBuilder configurationBuilder,
                                             string configurationFileName = "appsettings.json",
                                             string connectionStringName = "DefaultConnection")
    {
        return () => configurationBuilder.AddJsonFile(configurationFileName).Build().GetConnectionString(connectionStringName);
    }
}