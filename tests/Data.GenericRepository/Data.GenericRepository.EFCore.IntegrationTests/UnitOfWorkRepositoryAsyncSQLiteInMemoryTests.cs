using AutoFixture.Xunit2;
using Objectivity.AutoFixture.XUnit2.AutoMoq.Attributes;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Data;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Model;
using Ploch.Common.Data.Model;
using Ploch.Common.Reflection;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests;

public class UnitOfWorkRepositoryAsyncSQLiteInMemoryTests : DataIntegrationTest<TestDbContext>
{
    [Theory]
    [AutoMockData]
    public async Task RepositoryAsync_and_UnitOfWorkAsync_add_and_query_by_id_should_create_entities_and_find_them([Frozen] Blog testBlog)
    {
        using var unitOfWork = CreateUnitOfWork();
        testBlog.ExecuteOnProperties<IHasIdSettable<int>>(o => o.Id = 0);
        await unitOfWork.Repository<Blog, int>().AddAsync(testBlog);

        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        var userIdeas = await RepositoryHelper.AddAsyncTestUserIdeasEntitiesAsync(unitOfWork.Repository<UserIdea, int>());

        await unitOfWork.CommitAsync();

        var unitOfWork2 = CreateUnitOfWork();

        var blogRepository = CreateReadRepositoryAsync<Blog, int>();

        var actualBlog = await blogRepository.GetByIdAsync(blog.Id);
        actualBlog.Should().BeEquivalentTo(blog);

        var actualBlogPost1 = await unitOfWork2.Repository<BlogPost, int>().GetByIdAsync(blogPost1.Id);
        actualBlogPost1.Should().BeEquivalentTo(blogPost1);

        var actualBlogPost2 = await unitOfWork2.Repository<BlogPost, int>().GetByIdAsync(blogPost2.Id);
        actualBlogPost2.Should().BeEquivalentTo(blogPost2);

        var testUnitOfWork = CreateUnitOfWork();

        var actualIdeas = await testUnitOfWork.Repository<UserIdea, int>().GetAllAsync();

        actualIdeas.Should().HaveCount(2);

        actualIdeas.Should().BeEquivalentTo(userIdeas);
    }

    [Fact]
    public async Task UpdateAsync_entity()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var blogUpdated = new Blog { Id = blog.Id, Name = "Updated Blog" };

        await unitOfWork.Repository<Blog, int>().UpdateAsync(blogUpdated);

        var blogRepository = CreateReadRepositoryAsync<Blog, int>();

        var actualBlog = await blogRepository.GetByIdAsync(blog.Id);
        blog.Name = "Updated Blog";
        actualBlog.Should().BeEquivalentTo(blog);
    }

    [Fact]
    public async Task AddAsync_entity()
    {
        using var unitOfWork = CreateUnitOfWork();

        var repository = unitOfWork.Repository<Blog, int>();
        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(repository);

        await unitOfWork.CommitAsync();

        var blogRepository = CreateReadRepositoryAsync<Blog, int>();

        var actualBlog = await blogRepository.GetByIdAsync(blog.Id);
        actualBlog.Should().BeEquivalentTo(blog);
    }

    [Fact]
    public async Task DeleteAsync_entity()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var actualBlog = await unitOfWork.Repository<Blog, int>().GetByIdAsync(blog.Id);
        await unitOfWork.Repository<Blog, int>().DeleteAsync(actualBlog);
        await unitOfWork.CommitAsync();

        var deletedBlog = await unitOfWork.Repository<Blog, int>().GetByIdAsync(blog.Id);
        deletedBlog.Should().BeNull();

        var deletedBloPost1 = await unitOfWork.Repository<BlogPost, int>().GetByIdAsync(blogPost1.Id);
        deletedBloPost1.Should().BeNull();

        var deletedBloPost2 = await unitOfWork.Repository<BlogPost, int>().GetByIdAsync(blogPost2.Id);
        deletedBloPost2.Should().BeNull();
    }

    [Fact]
    public async Task Delete_entity()
    {
        using var unitOfWork = CreateUnitOfWork();

        var repositoryAsync = unitOfWork.Repository<Blog, int>();

        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(repositoryAsync);

        await unitOfWork.CommitAsync();

        var actualBlog = await repositoryAsync.GetByIdAsync(blog.Id);
        await repositoryAsync.DeleteAsync(actualBlog);
        await unitOfWork.CommitAsync();

        var deletedBlog = await repositoryAsync.GetByIdAsync(blog.Id);
        deletedBlog.Should().BeNull();

        var deletedBloPost1 = await unitOfWork.Repository<BlogPost, int>().GetByIdAsync(blogPost1.Id);
        deletedBloPost1.Should().BeNull();

        var deletedBloPost2 = await unitOfWork.Repository<BlogPost, int>().GetByIdAsync(blogPost2.Id);
        deletedBloPost2.Should().BeNull();
    }
}