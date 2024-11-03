using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class Blog : IHasId<int>, INamed
{
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

    public int Id { get; set; }

    public required string Name { get; set; }
}