using System;
using Microsoft.Extensions.Configuration;

namespace Ploch.Common.Data.EFCore;

public static class ConnectionString
{
    public static Func<string?> FromJsonFile(string configurationFileName = "appsettings.json", string connectionStringName = "DefaultConnection")
    {
        return FromJsonFile(new ConfigurationBuilder(), configurationFileName, connectionStringName);
    }

    public static Func<string?> FromJsonFile(ConfigurationBuilder configurationBuilder,
                                             string configurationFileName = "appsettings.json",
                                             string connectionStringName = "DefaultConnection")
    {
        return () => configurationBuilder.AddJsonFile(configurationFileName).Build().GetConnectionString(connectionStringName);
    }
}