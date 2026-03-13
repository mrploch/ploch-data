using Ploch.Data.GenericRepository.EFCore;

namespace Ploch.Data.GenericRepository.Tests;

public class RepositoriesConfigurationTests
{
    [Fact]
    public void Default_values_should_have_auditing_enabled_and_access_disabled()
    {
        var config = new RepositoriesConfiguration();

        config.EnableAuditing.Should().BeTrue();
        config.AuditAccess.Should().BeFalse();
    }

    [Fact]
    public void EnableAuditing_should_be_settable()
    {
        var config = new RepositoriesConfiguration { EnableAuditing = false };

        config.EnableAuditing.Should().BeFalse();
    }

    [Fact]
    public void AuditAccess_should_be_settable()
    {
        var config = new RepositoriesConfiguration { AuditAccess = true };

        config.AuditAccess.Should().BeTrue();
    }
}
