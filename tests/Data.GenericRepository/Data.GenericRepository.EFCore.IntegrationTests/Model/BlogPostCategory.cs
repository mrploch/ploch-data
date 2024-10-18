using Ploch.Data.Model.CommonTypes;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

public class BlogPostCategory : Category<BlogPostCategory, int>
{
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}