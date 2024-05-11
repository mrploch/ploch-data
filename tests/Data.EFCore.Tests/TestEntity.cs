using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.EFCore.Tests;

public class TestEntity : IHasId<int>, INamed
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;
}