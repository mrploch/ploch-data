using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class UserIdea : IHasIdSettable<int>
{
    public required string Contents { get; set; }

    public int Id { get; set; }
}