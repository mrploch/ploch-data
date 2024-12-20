using Microsoft.EntityFrameworkCore;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Data;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadWriteRepositoryAsyncTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    private readonly IReadWriteRepositoryAsync<TestEntity, int> _repository;

    public ReadWriteRepositoryAsyncTests()
    {
        _repository = CreateReadWriteRepositoryAsync<TestEntity, int>();
    }

    [Fact]
    public async Task ApplySqLiteDateTimeOffsetPropertiesFix_should_handle_DateTimeOffset_fields_in_SqLite()
    {
        using var unitOfWork = CreateUnitOfWork();

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepositoryAsync<BlogPost, int>();

        var blogPosts = await repository.GetAllAsync();

        foreach (var blogPost in blogPosts)
        {
            blogPost.CreatedTime.Should().BeBefore(DateTimeOffset.Now).And.BeAfter(DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)));
            blogPost.ModifiedTime.Should().BeBefore(DateTimeOffset.Now).And.BeAfter(DateTimeOffset.Now.Subtract(TimeSpan.FromHours(1)));
        }
    }

    [Fact]
    public async Task CountAsync_should_return_entity_count()
    {
        using var unitOfWork = CreateUnitOfWork();

        await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepositoryAsync<BlogPost, int>();
        var count = await repository.CountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_should_return_entities_with_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (_, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadWriteRepositoryAsync<BlogPost, int>();
        var blogPosts = await repository.GetAllAsync(onDbSet: query => query.Include(e => e.Tags));
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
    public async Task GetAllAsync_should_return_entities_without_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (_, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadWriteRepositoryAsync<BlogPost, int>();
        var blogPosts = await repository.GetAllAsync();
        blogPosts.Should().HaveCount(2);
        blogPosts.Should().ContainEquivalentOf(blogPost1, options => options.Excluding(p => p.Categories).Excluding(p => p.Tags));
        blogPosts.Should().ContainEquivalentOf(blogPost2, options => options.Excluding(p => p.Categories).Excluding(p => p.Tags));

        // Categories and Tags are not included in the query but they will not be empty because they are already loaded in the context.
        // This is why I don't check for emptiness here.
    }

    [Fact]
    public async Task GetPageAsync_should_return_a_page_of_entities_with_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(unitOfWork.Repository<Blog, int>(), 20);

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepositoryAsync<BlogPost, int>();
        var blogPosts = await repository.GetPageAsync(2, 5, onDbSet: query => query.Include(e => e.Tags).Include(e => e.Categories));

        blogPosts.Should().HaveCount(5);

        for (var i = 0; i < 5; i++)
        {
            var blogPost = posts[i + 5];
            var queriedPost = blogPosts.Should().ContainEquivalentOf(blogPost, options => options.Excluding(p => p.Categories).Excluding(p => p.Tags)).Subject;
            queriedPost.Tags.Should().BeEquivalentTo(blogPost.Tags, options => options.Excluding(t => t.BlogPosts));
            queriedPost.Categories.Should().HaveCount(blogPost.Categories.Count);
            queriedPost.Categories.Should()
                       .BeEquivalentTo(blogPost.Categories, options => options.Excluding(c => c.BlogPosts).Excluding(c => c.Parent).Excluding(c => c.Children));
        }
    }

    [Fact]
    public async Task GetPageAsync_should_return_a_page_of_entities_with_includes_using_query()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(unitOfWork.Repository<Blog, int>(), 20);

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepositoryAsync<BlogPost, int>();

        var blogPosts =
            await repository.GetPageAsync(2, 3,
#pragma warning disable SA1117 // Parameters should be placed on the same line
                                          query => query.Name == "Blog post 5" || query.Name == "Blog post 6" || query.Name == "Blog post 7" ||
                                                   query.Name == "Blog post 8" || query.Name == "Blog post 9" || query.Name == "Blog post 10",
#pragma warning restore SA1117
                                          query => query.Include(e => e.Tags).Include(e => e.Categories));

        blogPosts.Should().HaveCount(3);

        for (var i = 7; i <= 9; i++)
        {
            var blogPost = posts[i];
            var queriedPost = blogPosts.Should().ContainEquivalentOf(blogPost, options => options.Excluding(p => p.Categories).Excluding(p => p.Tags)).Subject;
            queriedPost.Tags.Should().BeEquivalentTo(blogPost.Tags, options => options.Excluding(t => t.BlogPosts));
            queriedPost.Categories.Should().HaveCount(blogPost.Categories.Count);
            queriedPost.Categories.Should()
                       .BeEquivalentTo(blogPost.Categories, options => options.Excluding(c => c.BlogPosts).Excluding(c => c.Parent).Excluding(c => c.Children));
        }
    }

    [Fact]
    public async Task GetPageAsync_should_return_a_page_of_entities_without_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(unitOfWork.Repository<Blog, int>(), 20);

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepositoryAsync<BlogPost, int>();
        var blogPosts = await repository.GetPageAsync(2, 5);

        blogPosts.Should().HaveCount(5);

        for (var i = 0; i < 5; i++)
        {
            var blogPost = posts[i + 5];
            var queriedPost = blogPosts.Should().ContainEquivalentOf(blogPost, options => options.Excluding(p => p.Categories).Excluding(p => p.Tags)).Subject;

            queriedPost.Categories.Should().BeEmpty();
            queriedPost.Tags.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task GetByIdAsync_should_return_entity_with_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (_, _, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepositoryAsync<BlogPost, int>();
        var blogPost = await repository.GetByIdAsync(blogPost2.Id, query => query.Include(e => e.Tags));
        blogPost.Should().BeEquivalentTo(blogPost2);
        blogPost!.Tags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_with_object_key_should_return_entity_with_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (_, _, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepositoryAsync<BlogPost, int>();
        var blogPost = await repository.GetByIdAsync([blogPost2.Id]);
        blogPost.Should().BeEquivalentTo(blogPost2);
        blogPost!.Tags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Count_should_return_zero_when_repository_is_empty()
    {
        using var unitOfWork = CreateUnitOfWork();

        var repository = CreateReadRepository<BlogPost, int>();
        var count = repository.Count();

        count.Should().Be(0);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var result = await _repository.AddAsync(entity);
        result.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddEntities()
    {
        var entities = new List<TestEntity> { new() { Id = 1, Name = "Test1" }, new() { Id = 2, Name = "Test2" } };
        var result = await _repository.AddRangeAsync(entities);
        result.Should().BeEquivalentTo(entities);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEntity()
    {
        var entity = new TestEntity { Id = 1, Name = "Test" };
        await _repository.AddAsync(entity);
        await _repository.DeleteAsync(entity);
        var result = await _repository.GetByIdAsync(entity.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        var entity = new TestEntity { Id = 1, Name = "Test" };
        await _repository.AddAsync(entity);
        entity.Name = "Updated";
        await _repository.UpdateAsync(entity);
        var result = await _repository.GetByIdAsync(entity.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateAsync_should_throw_if_updated_entity_doesnt_exist()
    {
        var entity = new TestEntity { Id = 1, Name = "Test" };
        await _repository.AddAsync(entity);

        var updatedEntity = new TestEntity { Id = 2, Name = "Updated" };
        var updateAction = async () => await _repository.UpdateAsync(updatedEntity);
        await updateAction.Should().ThrowAsync<InvalidOperationException>().Where(exception => exception.Message.Contains("not found"));
    }

    [Fact]
    public async Task FindFirstAsync_should_execute_query_and_return_the_first_hit()
    {
        using var unitOfWork = CreateUnitOfWork();

        var testBlogEntities = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadWriteRepositoryAsync<BlogPost, int>();
        var blogPost = await repository.FindFirstAsync(post => post.Name.Contains("Blog post 1"));

        blogPost.Should().NotBeNull();
        blogPost.Should().BeEquivalentTo(testBlogEntities.blogPost1);
    }
}