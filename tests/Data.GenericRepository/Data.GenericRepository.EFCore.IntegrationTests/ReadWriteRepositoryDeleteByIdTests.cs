using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadWriteRepositoryDeleteByIdTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public async Task Delete_by_id_should_remove_entity()
    {
        using var unitOfWork = CreateUnitOfWork();
        var asyncRepo = unitOfWork.Repository<TestEntity, int>();
        await asyncRepo.AddAsync(new TestEntity { Id = 1, Name = "ToDelete" });
        await unitOfWork.CommitAsync();

        var repository = CreateReadWriteRepository<TestEntity, int>();
        repository.Delete(1);

        // After committing, it should be gone from the database.
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var result = repository.GetById(1);
        result.Should().BeNull();
    }

    [Fact]
    public void Delete_by_id_should_throw_EntityNotFoundException_when_entity_does_not_exist()
    {
        var repository = CreateReadWriteRepository<TestEntity, int>();

        var act = () => repository.Delete(999);

        act.Should().Throw<EntityNotFoundException>().Where(e => e.Message.Contains("not found"));
    }

    [Fact]
    public void GetById_should_return_null_when_entity_does_not_exist()
    {
        var repository = CreateReadRepository<TestEntity, int>();

        var result = repository.GetById(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetById_with_onDbSet_should_return_entity()
    {
        using var unitOfWork = CreateUnitOfWork();
        var asyncRepo = unitOfWork.Repository<TestEntity, int>();
        await asyncRepo.AddAsync(new TestEntity { Id = 1, Name = "WithOnDbSet" });
        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<TestEntity, int>();
        var result = repository.GetById(1, q => q.Where(e => e.Name.Contains("WithOnDbSet")));

        result.Should().NotBeNull();
        result!.Name.Should().Be("WithOnDbSet");
    }

    [Fact]
    public async Task GetById_with_onDbSet_should_return_null_when_filter_excludes_entity()
    {
        using var unitOfWork = CreateUnitOfWork();
        var asyncRepo = unitOfWork.Repository<TestEntity, int>();
        await asyncRepo.AddAsync(new TestEntity { Id = 1, Name = "Excluded" });
        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<TestEntity, int>();
        var result = repository.GetById(1, q => q.Where(e => e.Name == "NonExistent"));

        result.Should().BeNull();
    }
}
