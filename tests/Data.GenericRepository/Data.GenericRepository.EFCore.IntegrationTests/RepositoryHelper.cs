using Microsoft.EntityFrameworkCore;
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

    /// <summary>
    /// Seeds a blog with many blog posts directly via the <see cref="DbContext" /> and returns the seeded entities.
    /// </summary>
    /// <remarks>
    /// Use this overload in the Arrange phase of an integration test. Seeding through the plain
    /// <see cref="DbContext" /> — rather than through a repository — keeps the test's fixture setup
    /// independent of the Generic Repository write path, which is itself code under test.
    /// </remarks>
    /// <param name="dbContext">The <see cref="DbContext" /> to add the blog to.</param>
    /// <param name="blogPostCount">The number of blog posts to attach to the blog.</param>
    /// <returns>
    /// A task that resolves to a tuple of the seeded <see cref="Blog" /> and the array of <see cref="BlogPost" />
    /// instances attached to it.
    /// </returns>
    public static async Task<(Blog blog, BlogPost[] blogPosts)> AddAsyncTestBlogEntitiesWithManyPostsAsync(
        DbContext dbContext,
        int blogPostCount)
    {
        var (blog, blogPosts) = EntitiesBuilder.BuildBlogEntityWithManyPosts(blogPostCount);

        await dbContext.AddAsync(blog);
        await dbContext.SaveChangesAsync();

        return (blog, blogPosts);
    }

    /// <summary>
    /// Seeds a blog with many blog posts via a short-lived <see cref="DbContext" /> obtained from
    /// <paramref name="dbContextFactory" /> and returns the seeded entities.
    /// </summary>
    /// <remarks>
    /// The context produced by <paramref name="dbContextFactory" /> is created, used, and disposed
    /// inside this method. Pass <c>CreateRootDbContext</c> so the fixture is seeded through a context
    /// separate from the one the code under test reads through — this keeps the seeded entities out
    /// of the read context's change tracker without the caller managing a context's lifetime or
    /// remembering to clear the tracker.
    /// </remarks>
    /// <param name="dbContextFactory">A factory that produces a new <see cref="DbContext" /> to seed through.</param>
    /// <param name="blogPostCount">The number of blog posts to attach to the blog.</param>
    /// <returns>
    /// A task that resolves to a tuple of the seeded <see cref="Blog" /> and the array of <see cref="BlogPost" />
    /// instances attached to it.
    /// </returns>
    public static async Task<(Blog blog, BlogPost[] blogPosts)> AddAsyncTestBlogEntitiesWithManyPostsAsync(
        Func<DbContext> dbContextFactory,
        int blogPostCount)
    {
        await using var dbContext = dbContextFactory();

        return await AddAsyncTestBlogEntitiesWithManyPostsAsync(dbContext, blogPostCount);
    }

    public static async Task<(Blog blog, BlogPost blogPost1, BlogPost blogPost2)> AddAsyncTestBlogEntitiesAsync(
        IReadWriteRepositoryAsync<Blog, int> blogReadWriteRepository,
        int numTags = 3)
    {
        var (blog, blogPost1, blogPost2) = EntitiesBuilder.BuildBlogEntity(numTags);

        await blogReadWriteRepository.AddAsync(blog);

        return (blog, blogPost1, blogPost2);
    }

    /// <summary>
    /// Seeds a blog with two blog posts directly via the <see cref="DbContext" /> and returns the seeded entities.
    /// </summary>
    /// <remarks>
    /// Use this overload in the Arrange phase of an integration test. Seeding through the plain
    /// <see cref="DbContext" /> — rather than through a repository — keeps the test's fixture setup
    /// independent of the Generic Repository write path, which is itself code under test.
    /// </remarks>
    /// <param name="dbContext">The <see cref="DbContext" /> to add the blog to.</param>
    /// <param name="numTags">The number of tags to attach to each blog post. Defaults to 3.</param>
    /// <returns>
    /// A task that resolves to a tuple of the seeded <see cref="Blog" /> and the two <see cref="BlogPost" />
    /// instances attached to it.
    /// </returns>
    /// <example>
    /// <code>
    /// var (blog, post1, post2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(DbContext);
    /// </code>
    /// </example>
    public static async Task<(Blog blog, BlogPost blogPost1, BlogPost blogPost2)> AddAsyncTestBlogEntitiesAsync(
        DbContext dbContext,
        int numTags = 3)
    {
        var (blog, blogPost1, blogPost2) = EntitiesBuilder.BuildBlogEntity(numTags);

        await dbContext.AddAsync(blog);
        await dbContext.SaveChangesAsync();

        return (blog, blogPost1, blogPost2);
    }

    /// <summary>
    /// Seeds a blog with two blog posts via a short-lived <see cref="DbContext" /> obtained from
    /// <paramref name="dbContextFactory" /> and returns the seeded entities.
    /// </summary>
    /// <remarks>
    /// The context produced by <paramref name="dbContextFactory" /> is created, used, and disposed
    /// inside this method. Pass <c>CreateRootDbContext</c> so the fixture is seeded through a context
    /// separate from the one the code under test reads through — this keeps the seeded entities out
    /// of the read context's change tracker without the caller managing a context's lifetime or
    /// remembering to clear the tracker.
    /// </remarks>
    /// <param name="dbContextFactory">A factory that produces a new <see cref="DbContext" /> to seed through.</param>
    /// <param name="numTags">The number of tags to attach to each blog post. Defaults to 3.</param>
    /// <returns>
    /// A task that resolves to a tuple of the seeded <see cref="Blog" /> and the two <see cref="BlogPost" />
    /// instances attached to it.
    /// </returns>
    /// <example>
    /// <code>
    /// var (blog, post1, post2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(CreateRootDbContext);
    /// </code>
    /// </example>
    public static async Task<(Blog blog, BlogPost blogPost1, BlogPost blogPost2)> AddAsyncTestBlogEntitiesAsync(
        Func<DbContext> dbContextFactory,
        int numTags = 3)
    {
        await using var dbContext = dbContextFactory();

        return await AddAsyncTestBlogEntitiesAsync(dbContext, numTags);
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

    /// <summary>
    /// Seeds a set of randomly-named blog-post tags directly via the <see cref="DbContext" /> and returns them.
    /// </summary>
    /// <param name="dbContext">The <see cref="DbContext" /> to add the tags to.</param>
    /// <param name="tagCount">The number of tags to create.</param>
    /// <returns>A task that resolves to the array of seeded <see cref="BlogPostTag" /> instances.</returns>
    public static async Task<BlogPostTag[]> AddBlogPostTagsAsync(DbContext dbContext, int tagCount)
    {
        var tags = EntitiesBuilder.BuildRandomTags(tagCount);

        await dbContext.AddRangeAsync(tags);
        await dbContext.SaveChangesAsync();

        return tags;
    }

    /// <summary>
    /// Seeds a set of randomly-named blog-post tags via a short-lived <see cref="DbContext" /> obtained
    /// from <paramref name="dbContextFactory" /> and returns them.
    /// </summary>
    /// <remarks>
    /// The context produced by <paramref name="dbContextFactory" /> is created, used, and disposed
    /// inside this method. Pass <c>CreateRootDbContext</c> to seed through a context separate from the
    /// one the code under test reads through.
    /// </remarks>
    /// <param name="dbContextFactory">A factory that produces a new <see cref="DbContext" /> to seed through.</param>
    /// <param name="tagCount">The number of tags to create.</param>
    /// <returns>A task that resolves to the array of seeded <see cref="BlogPostTag" /> instances.</returns>
    public static async Task<BlogPostTag[]> AddBlogPostTagsAsync(Func<DbContext> dbContextFactory, int tagCount)
    {
        await using var dbContext = dbContextFactory();

        return await AddBlogPostTagsAsync(dbContext, tagCount);
    }
}
