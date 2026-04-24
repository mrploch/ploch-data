using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

/// <summary>
/// Helpers for seeding repository-backed integration-test fixtures with a known set of blog, blog-post,
/// tag, category, and user-idea entities. The helpers wrap <see cref="EntitiesBuilder" /> so tests can
/// populate the database in a single call and get typed references back to the seeded entities for
/// later assertion.
/// </summary>
public static class RepositoryHelper
{
    /// <summary>
    /// Seeds a blog with two blog posts via the synchronous repository and returns the seeded entities.
    /// </summary>
    /// <param name="blogRepository">The synchronous <see cref="IReadWriteRepository{Blog,Int32}" /> to add the blog to.</param>
    /// <returns>
    /// A tuple of the seeded <see cref="Blog" /> and the two <see cref="BlogPost" /> instances attached to it.
    /// </returns>
    /// <example>
    /// <code>
    /// var (blog, post1, post2) = RepositoryHelper.AddTestBlogEntities(blogRepository);
    /// </code>
    /// </example>
    public static (Blog, BlogPost, BlogPost) AddTestBlogEntities(IReadWriteRepository<Blog, int> blogRepository)
    {
        var (blog, blogPost1, blogPost2) = EntitiesBuilder.BuildBlogEntity();

        blogRepository.Add(blog);

        return (blog, blogPost1, blogPost2);
    }

    /// <summary>
    /// Seeds a blog with two blog posts via the asynchronous repository and returns the seeded entities.
    /// </summary>
    /// <param name="blogRepository">The asynchronous <see cref="IReadWriteRepositoryAsync{Blog,Int32}" /> to add the blog to.</param>
    /// <returns>
    /// A task that resolves to a tuple of the seeded <see cref="Blog" /> and the two <see cref="BlogPost" />
    /// instances attached to it.
    /// </returns>
    /// <example>
    /// <code>
    /// var (blog, post1, post2) = await RepositoryHelper.AddTestBlogEntities(blogRepository);
    /// </code>
    /// </example>
    public static async Task<(Blog, BlogPost, BlogPost)> AddTestBlogEntities(IReadWriteRepositoryAsync<Blog, int> blogRepository)
    {
        var (blog, blogPost1, blogPost2) = EntitiesBuilder.BuildBlogEntity();

        await blogRepository.AddAsync(blog);

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
