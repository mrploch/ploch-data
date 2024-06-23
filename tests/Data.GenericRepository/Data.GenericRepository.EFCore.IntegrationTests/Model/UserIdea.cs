using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class UserIdea : IHasIdSettable<int>
{
    public required string Contents { get; set; }

    public int Id { get; set; }
}