using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class TestEntity : IHasId<int>, INamed
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
}