using Ploch.Data.Model;

namespace Ploch.Data.EFCore.Tests;

public class TestEntity : IHasId<int>, INamed
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
}