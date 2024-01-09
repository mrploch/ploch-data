using Ploch.Common.Data.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class BlogPost : IHasIdSettable<int>, INamed
{
    public string? Contents { get; set; }

    public virtual ICollection<BlogPostCategory> Categories { get; set; } = new List<BlogPostCategory>();

    public virtual ICollection<BlogPostTag> Tags { get; set; } = new List<BlogPostTag>();

    public int Id { get; set; }

    public string Name { get; set; } = default!;
}