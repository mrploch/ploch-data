using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Data;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

// TODO: Fix those tests - use UnitOfWork and test against the real data
public class ReadWriteRepositoryTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    private readonly IReadWriteRepository<TestEntity, int> _repository;

    public ReadWriteRepositoryTests() => _repository = CreateReadWriteRepository<TestEntity, int>();

    [Fact]
    public void AddAsync_ShouldAddEntity()
    {
        var entity = new TestEntity { Id = 1, Name = "Test" };
        var result = _repository.Add(entity);
        result.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void AddRangeAsync_ShouldAddEntities()
    {
        var entities = new List<TestEntity> { new() { Id = 1, Name = "Test1" }, new() { Id = 2, Name = "Test2" } };
        var result = _repository.AddRange(entities);
        result.Should().BeEquivalentTo(entities);
    }

    [Fact]
    public void DeleteAsync_ShouldDeleteEntity()
    {
        var entity = new TestEntity { Id = 1, Name = "Test" };
        _repository.Add(entity);
        _repository.Delete(entity);
        var result = _repository.GetById(entity.Id);
        result.Should().BeNull();
    }

    [Fact]
    public void UpdateAsync_ShouldUpdateEntity()
    {
        var entity = new TestEntity { Id = 1, Name = "Test" };
        _repository.Add(entity);
        entity.Name = "Updated";
        _repository.Update(entity);
        var result = _repository.GetById(entity.Id);
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated");
    }

    [Fact]
    public void UpdateAsync_should_throw_if_updated_entity_doesnt_exist()
    {
        var entity = new TestEntity { Id = 1, Name = "Test" };
        _repository.Add(entity);

        var updatedEntity = new TestEntity { Id = 2, Name = "Updated" };
        var updateAction = () => _repository.Update(updatedEntity);
        updateAction.Should().Throw<EntityNotFoundException>().Where(exception => exception.Message.Contains("not found"));
    }
}
