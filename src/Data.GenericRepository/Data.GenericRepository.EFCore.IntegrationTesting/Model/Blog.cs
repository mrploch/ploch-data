using Ploch.Data.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class Blog : IHasId<int>, INamed, IHasAuditProperties
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public DateTimeOffset? ModifiedTime { get; set; }

    public DateTimeOffset? AccessedTime { get; set; }

    public DateTimeOffset? CreatedTime { get; set; }

    public string? LastModifiedBy { get; set; }

    public string? CreatedBy { get; set; }

    public string? LastAccessedBy { get; set; }

    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
