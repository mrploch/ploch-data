using Microsoft.EntityFrameworkCore;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Data;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadRepositoryTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    private readonly IReadRepository<TestEntity, int> _repository;
    
    public ReadRepositoryTests()
    {
        _repository = CreateReadRepository<TestEntity, int>();
    }
    
    [Fact]
    public async Task GetAll_should_return_entities_with_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPosts = repository.GetAll(query => query.Include(e => e.Tags));
        blogPosts.Should().HaveCount(2);
        blogPosts.Should().ContainEquivalentOf(blogPost1);
        blogPosts.Should().ContainEquivalentOf(blogPost2);
        foreach (var blogPost in blogPosts)
        {
            blogPost.Tags.Should().NotBeEmpty();
            blogPost.Tags.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task GetById_should_return_entity_with_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPost = repository.GetById(blogPost2.Id, query => query.Include(e => e.Tags));
        blogPost.Should().BeEquivalentTo(blogPost2);
        blogPost2.Tags.Should().NotBeEmpty();
    }
}