using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class UnitOfWorkAdditionalTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public async Task RollbackAsync_should_revert_uncommitted_changes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new TestEntity { Id = 1, Name = "ToBeRolledBack" });
        await unitOfWork.CommitAsync();

        // Modify the entity
        var entity = await repository.GetByIdAsync(1);
        entity.Should().NotBeNull();
        entity!.Name = "Modified";

        // Rollback before committing
        await unitOfWork.RollbackAsync();

        var reloaded = await repository.GetByIdAsync(1);
        reloaded.Should().NotBeNull();
        reloaded!.Name.Should().Be("ToBeRolledBack");
    }

    [Fact]
    public void Repository_should_cache_and_return_same_instance()
    {
        using var unitOfWork = CreateUnitOfWork();

        var repo1 = unitOfWork.Repository<TestEntity, int>();
        var repo2 = unitOfWork.Repository<TestEntity, int>();

        repo1.Should().BeSameAs(repo2);
    }

    [Fact]
    public void Dispose_should_not_throw()
    {
        var unitOfWork = CreateUnitOfWork();

        unitOfWork.Invoking(u => u.Dispose()).Should().NotThrow();
    }

    [Fact]
    public async Task DisposeAsync_should_not_throw()
    {
        var unitOfWork = CreateUnitOfWork();

        if (unitOfWork is IAsyncDisposable asyncDisposable)
        {
            var act = async () => await asyncDisposable.DisposeAsync();
            await act.Should().NotThrowAsync();
        }
    }

    [Fact]
    public async Task CommitAsync_with_no_changes_should_return_zero()
    {
        using var unitOfWork = CreateUnitOfWork();

        var result = await unitOfWork.CommitAsync();

        result.Should().Be(0);
    }

    [Fact]
    public async Task CommitAsync_with_changes_should_return_number_of_affected_entities()
    {
        using var unitOfWork = CreateUnitOfWork();

        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new TestEntity { Id = 1, Name = "Entity1" });
        await repository.AddAsync(new TestEntity { Id = 2, Name = "Entity2" });

        var result = await unitOfWork.CommitAsync();

        result.Should().Be(2);
    }
}
