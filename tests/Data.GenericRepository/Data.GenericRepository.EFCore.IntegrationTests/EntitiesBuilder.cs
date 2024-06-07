using AutoFixture;
using Ploch.Common.Collections;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests;

public static class EntitiesBuilder
{
    public static BlogPostTag[] BuildRandomTags(int count)
    {
        var fixture = new Fixture();

        return fixture.Build<BlogPostTag>().Without(t => t.BlogPosts).Without(t => t.Id).CreateMany(count).ToArray();
    }

    public static (Blog, BlogPost, BlogPost) BuildBlogEntity()
    {
        var categories = BuildCategories();

        var tags = BuildRandomTags(3);

        var blog = new Blog { Name = "Blog 1" };

        var blogPost1 = new BlogPost { Name = "Blog post 1", Contents = "My first blog post!", CreatedTime = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(1)), ModifiedTime = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(1))};
        blogPost1.Categories.AddMany(categories.TakeRandom(2));
        blogPost1.Tags.Add(tags[0]);
        blogPost1.Tags.Add(tags[2]);

        var blogPost2 = new BlogPost { Name = "Blog post 2", Contents = "My second blog post!", CreatedTime = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(2)), ModifiedTime = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(2)) };
        blogPost2.Tags.Add(tags[1]);

        blog.BlogPosts.Add(blogPost1);
        blog.BlogPosts.Add(blogPost2);

        return (blog, blogPost1, blogPost2);
    }

    public static (Blog, BlogPost[]) BuildBlogEntityWithManyPosts(int blogPostCount)
    {
        var blog = new Blog { Name = "Blog 1" };

        var blogPosts = BuildBlogPosts(blogPostCount);

        blog.BlogPosts.AddMany(blogPosts);

        return (blog, blogPosts.ToArray());
    }

    public static IEnumerable<BlogPostCategory> BuildCategories()
    {
        var category1 = new BlogPostCategory { Name = "Category 1" };
        var category1_1 = new BlogPostCategory { Name = "Category 1.1", Parent = category1 };
        var category1_2 = new BlogPostCategory { Name = "Category 1.2", Parent = category1 };
        var category1_2_1 = new BlogPostCategory { Name = "Category 1.2.1", Parent = category1_2 };

        var category2 = new BlogPostCategory { Name = "Category 2" };

        return new List<BlogPostCategory>
               {
                   category1,
                   category1_1,
                   category1_2,
                   category1_2_1,
                   category2
               };
    }

    public static IEnumerable<BlogPost> BuildBlogPosts(int count)
    {
        var category1 = new BlogPostCategory { Name = "Category 1" };
        var category1_1 = new BlogPostCategory { Name = "Category 1.1", Parent = category1 };
        var category1_2 = new BlogPostCategory { Name = "Category 1.2", Parent = category1 };
        var category1_2_1 = new BlogPostCategory { Name = "Category 1.2.1", Parent = category1_2 };

        var category2 = new BlogPostCategory { Name = "Category 2" };

        var categories = new List<BlogPostCategory>
                         {
                             category1,
                             category1_1,
                             category1_2,
                             category1_2_1,
                             category2
                         };

        var tags = BuildRandomTags(3);

        var posts = new List<BlogPost>();

        for (var i = 0; i < count; i++)
        {
            var post = new BlogPost { Name = $"Blog post {i + 1}", Contents = $"My blog post {i + 1}" };
            post.Categories.AddMany(categories.TakeRandom(2));
            post.Tags.AddMany(tags.TakeRandom(2));

            posts.Add(post);
        }

        return posts;
    }

    public static (UserIdea, UserIdea) BuildUserIdeaEntities()
    {
        var userIdea1 = new UserIdea { Contents = "My first idea!" };
        var userIdea2 = new UserIdea { Contents = "My second idea!" };

        return (userIdea1, userIdea2);
    }
}