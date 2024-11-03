using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public static class RepositoryHelper
{
    public static (Blog, BlogPost, BlogPost) AddTestBlogEntities(IReadWriteRepository<Blog, int> blogRepository)
    {
        var (blog, blogPost1, blogPost2) = EntitiesBuilder.BuildBlogEntity();

        blogRepository.Add(blog);

        return (blog, blogPost1, blogPost2);
    }

    public static IEnumerable<UserIdea> AddTestUserIdeasEntities(IReadWriteRepository<UserIdea, int> userIdeasRepository)
    {
        var (userIdea1, userIdea2) = EntitiesBuilder.BuildUserIdeaEntities();

        userIdeasRepository.Add(userIdea1);
        userIdeasRepository.Add(userIdea2);

        return new[] { userIdea1, userIdea2 };
    }

    public static async Task<(Blog blog, BlogPost[] blogPosts)> AddAsyncTestBlogEntitiesWithManyPostsAsync(IReadWriteRepositoryAsync<Blog, int> blogReadWriteRepository,
                                                                                                           int blogPostCount)
    {
        var (blog, blogPosts) = EntitiesBuilder.BuildBlogEntityWithManyPosts(blogPostCount);

        await blogReadWriteRepository.AddAsync(blog);

        return (blog, blogPosts);
    }

    // ReSharper disable once MethodOverloadWithOptionalParameter - false positive here
    public static async Task<(Blog blog, BlogPost blogPost1, BlogPost blogPost2)> AddAsyncTestBlogEntitiesAsync(
        IReadWriteRepositoryAsync<Blog, int> blogReadWriteRepository,
        int blogPostCount,
        int numTags = 3)
    {
        var (blog, blogPost1, blogPost2) = EntitiesBuilder.BuildBlogEntity(numTags);

        await blogReadWriteRepository.AddAsync(blog);

        return (blog, blogPost1, blogPost2);
    }

    public static async Task<(Blog blog, BlogPost blogPost1, BlogPost blogPost2)> AddAsyncTestBlogEntitiesAsync(
        IReadWriteRepositoryAsync<Blog, int> blogReadWriteRepository,
        int numTags = 3)
    {
        var (blog, blogPost1, blogPost2) = EntitiesBuilder.BuildBlogEntity(numTags);

        await blogReadWriteRepository.AddAsync(blog);

        return (blog, blogPost1, blogPost2);
    }

    public static async Task<IEnumerable<UserIdea>> AddAsyncTestUserIdeasEntitiesAsync(IReadWriteRepositoryAsync<UserIdea, int> userIdeasReadWriteRepository)
    {
        var (userIdea1, userIdea2) = EntitiesBuilder.BuildUserIdeaEntities();

        await userIdeasReadWriteRepository.AddAsync(userIdea1);
        await userIdeasReadWriteRepository.AddAsync(userIdea2);

        return new[] { userIdea1, userIdea2 };
    }

    public static async Task<BlogPostTag[]> AddBlogPostTagsAsync(IReadWriteRepositoryAsync<BlogPostTag, int> repository, int tagCount)
    {
        var tags = EntitiesBuilder.BuildRandomTags(tagCount);

        await repository.AddRangeAsync(tags);

        return tags;
    }
}