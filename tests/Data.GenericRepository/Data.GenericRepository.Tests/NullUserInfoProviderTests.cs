using Ploch.Data.GenericRepository.EFCore;

namespace Ploch.Data.GenericRepository.Tests;

public class NullUserInfoProviderTests
{
    [Fact]
    public void GetCurrentUserInfo_should_return_null()
    {
        var provider = new NullUserInfoProvider();

        provider.GetCurrentUserInfo().Should().BeNull();
    }
}
