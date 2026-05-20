using AutoFixture.Xunit3;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;
using Ploch.TestingSupport.XUnit3.AutoMoq;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class UnitOfWorkRepositoryAsyncSQLiteInMemoryTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Theory]
    [AutoMockData]
    public async Task RepositoryAsync_and_UnitOfWorkAsync_add_and_query_by_id_should_create_entities_and_find_them([Frozen] Blog testBlog)
    {
        using var unitOfWork = CreateUnitOfWork();
        testBlog.Id = 0;
        foreach (var testBlogBlogPost in testBlog.BlogPosts)
        {
            testBlogBlogPost.Id = 0;
            foreach (var category in testBlogBlogPost.Categories)
            {
                category.Id = 0;
            }

            foreach (var blogPostTag in testBlogBlogPost.Tags)
            {
                blogPostTag.Id = 0;
            }
        }

        // Act — adding entities through the unit of work's repositories and committing IS the
        // behaviour under test here, so this seeding deliberately stays repository-based.
        await unitOfWork.Repository<Blog, int>().AddAsync(testBlog);

        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        var userIdeas = await RepositoryHelper.AddAsyncTestUserIdeasEntitiesAsync(unitOfWork.Repository<UserIdea, int>());

        await unitOfWork.CommitAsync();

        using var unitOfWork2 = CreateUnitOfWork();

        var blogRepository = CreateReadRepositoryAsync<Blog, int>();

        var actualBlog = await blogRepository.GetByIdAsync(blog.Id);
        actualBlog.Should()
                  .BeEquivalentTo(blog,
                                  options => options.Excluding(p => p.BlogPosts).WithEntityEquivalencyOptions());
        actualBlog!.Name.Should().Be(blog.Name);

        var actualBlogPost1 = await unitOfWork2.Repository<BlogPost, int>().GetByIdAsync(blogPost1.Id);
        actualBlogPost1.Should()
                       .BeEquivalentTo(blogPost1,
                                       options => options.Excluding(p => p.Categories)
                                                         .Excluding(p => p.Tags)
                                                         .WithEntityEquivalencyOptions());

        var actualBlogPost2 = await unitOfWork2.Repository<BlogPost, int>().GetByIdAsync(blogPost2.Id);
        actualBlogPost2.Should()
                       .BeEquivalentTo(blogPost2,
                                       options => options.Excluding(p => p.Categories)
                                                         .Excluding(p => p.Tags)
                                                         .WithEntityEquivalencyOptions());

        using var testUnitOfWork = CreateUnitOfWork();

        var actualIdeas = await testUnitOfWork.Repository<UserIdea, int>().GetAllAsync();

        actualIdeas.Should().HaveCount(2);

        actualIdeas.Should().BeEquivalentTo(userIdeas);
    }

    [Fact]
    public async Task UpdateAsync_entity()
    {
        // Arrange — seed via a separate context so the fixture setup does not depend on the
        // repository write path; the update below is the operation under test.
        var (blog, _, _) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(CreateRootDbContext);

        using var unitOfWork = CreateUnitOfWork();
        var blogUpdated = new Blog { Id = blog.Id, Name = "Updated Blog" };

        await unitOfWork.Repository<Blog, int>().UpdateAsync(blogUpdated);
        await unitOfWork.CommitAsync();

        var blogRepository = CreateReadRepositoryAsync<Blog, int>();

        var actualBlog = await blogRepository.GetByIdAsync(blog.Id);
        blog.Name = "Updated Blog";

        // Audit timestamps are the concern of ReadWriteRepositoryAsyncAuditTests; UpdateAsync
        // legitimately sets ModifiedTime, so exclude the audit columns from this comparison.
        actualBlog.Should()
                  .BeEquivalentTo(blog,
                                  options => options.Excluding(p => p.BlogPosts)
                                                    .Excluding(p => p.CreatedTime)
                                                    .Excluding(p => p.ModifiedTime)
                                                    .WithEntityEquivalencyOptions());
    }

    [Fact]
    public async Task AddAsync_entity()
    {
        using var unitOfWork = CreateUnitOfWork();

        var repository = unitOfWork.Repository<Blog, int>();

        // Act — adding the blog through the repository is the operation under test, so this
        // call deliberately stays repository-based.
        var (blog, _, _) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(repository);

        await unitOfWork.CommitAsync();

        var blogRepository = CreateReadRepositoryAsync<Blog, int>();

        var actualBlog = await blogRepository.GetByIdAsync(blog.Id);
        actualBlog.Should()
                  .BeEquivalentTo(blog,
                                  options => options.Excluding(p => p.BlogPosts).WithEntityEquivalencyOptions());
    }

    [Fact]
    public async Task DeleteAsync_entity()
    {
        // Arrange — seed via a separate context so the fixture setup does not depend on the
        // repository write path; the delete below is the operation under test.
        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(CreateRootDbContext);

        using var unitOfWork = CreateUnitOfWork();
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
        // Arrange — seed via a separate context so the fixture setup does not depend on the
        // repository write path; the delete below is the operation under test.
        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(CreateRootDbContext);

        using var unitOfWork = CreateUnitOfWork();
        var repositoryAsync = unitOfWork.Repository<Blog, int>();
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
