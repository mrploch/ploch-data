using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Data;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadWriteRepositoryAsyncTests : DataIntegrationTest<TestDbContext>
{
    private readonly IReadWriteRepositoryAsync<TestEntity, int> _repository;

    public ReadWriteRepositoryAsyncTests()
    {
        _repository = CreateReadWriteRepositoryAsync<TestEntity, int>();
    }

    [Fact]
    public async Task CountAsync_should_return_entity_count()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepositoryAsync<BlogPost, int>();
        var count = await repository.GetCountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task Count_should_return_entity_count()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (blog, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<BlogPost, int>();
        var count = repository.Count();

        count.Should().Be(2);
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
        result.Name.Should().Be("Updated");
    }
}