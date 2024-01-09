using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class Blog : IHasId<int>, INamed
{
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

    public int Id { get; set; }

    public required string Name { get; set; }
}