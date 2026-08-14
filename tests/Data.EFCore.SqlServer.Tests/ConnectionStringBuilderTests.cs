using FluentAssertions;
using Microsoft.Data.SqlClient;

namespace Ploch.Data.EFCore.SqlServer.Tests;

public class ConnectionStringBuilderTests
{
    [Fact]
    public void SqlConnectionStringBuilder_should_build_expected_connection_string()
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = "localhost",
            InitialCatalog = "Ploch.Lists",
            TrustServerCertificate = true,
            IntegratedSecurity = true
        };
        var connectionString = builder.ToString();
        connectionString.Should().Be("Data Source=localhost;Initial Catalog=Ploch.Lists;Integrated Security=True;Trust Server Certificate=True");
    }
}