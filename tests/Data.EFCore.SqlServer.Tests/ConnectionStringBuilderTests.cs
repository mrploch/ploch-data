using FluentAssertions;
using Microsoft.Data.SqlClient;

namespace Ploch.Data.EFCore.SqlServer.Tests;

public class ConnectionStringBuilderTests
{
    [Fact]
    public void MyMethod()
    {
        var builder = new SqlConnectionStringBuilder();
        builder.DataSource = "localhost";
        builder.InitialCatalog = "Ploch.Lists";
        builder.TrustServerCertificate = true;
        builder.IntegratedSecurity = true;
        var connectionString = builder.ToString();
        connectionString.Should().Be("Data Source=localhost;Initial Catalog=Ploch.Lists;Integrated Security=True;Trust Server Certificate=True");
    }
}