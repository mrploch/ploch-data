using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class UserIdea : IHasIdSettable<int>
{
    public int Id { get; set; }

    public required string Contents { get; set; }
}
