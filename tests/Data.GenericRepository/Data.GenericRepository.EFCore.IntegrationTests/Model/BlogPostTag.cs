using Ploch.Common.Data.Model.CommonTypes;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class BlogPostTag : Tag
{
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}