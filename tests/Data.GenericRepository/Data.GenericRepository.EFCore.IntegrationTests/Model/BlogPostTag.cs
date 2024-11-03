using Ploch.Data.Model.CommonTypes;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class BlogPostTag : Tag
{
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}