using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class BlogPost : IHasIdSettable<int>, INamed, IHasCreatedTime, IHasModifiedTime
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public DateTimeOffset? CreatedTime { get; set; }

    public DateTimeOffset? ModifiedTime { get; set; }

    public string? Contents { get; set; }

    public virtual ICollection<BlogPostCategory> Categories { get; set; } = new List<BlogPostCategory>();

    public virtual ICollection<BlogPostTag> Tags { get; set; } = new List<BlogPostTag>();
}
