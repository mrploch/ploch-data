using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests;

public class RepositoryHelper
{
    public static (Blog, BlogPost, BlogPost) AddTestBlogEntitiesAsync(IReadWriteRepository<Blog, int> blogRepository)
    {
        var (blog, blogPost1, blogPost2) = EntitiesBuilder.BuildBlogEntity();

        blogRepository.Add(blog);

        return (blog, blogPost1, blogPost2);
    }

    public static IEnumerable<UserIdea> AddTestUserIdeasEntitiesAsync(IReadWriteRepository<UserIdea, int> userIdeasRepository)
    {
        var (userIdea1, userIdea2) = EntitiesBuilder.BuildUserIdeaEntities();

        userIdeasRepository.Add(userIdea1);
        userIdeasRepository.Add(userIdea2);

        return new[] { userIdea1, userIdea2 };
    }

    public static async Task<(Blog blog, BlogPost blogPost1, BlogPost blogPost2)> AddAsyncTestBlogEntitiesAsync(
        IReadWriteRepositoryAsync<Blog, int> blogReadWriteRepository)
    {
        var (blog, blogPost1, blogPost2) = EntitiesBuilder.BuildBlogEntity();

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
}