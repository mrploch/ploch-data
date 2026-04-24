using Microsoft.EntityFrameworkCore;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadWriteRepositoryAsyncAdditionalTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public async Task DeleteAsync_by_id_should_remove_entity()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "ToDelete" });
        await unitOfWork.CommitAsync();

        await repository.DeleteAsync(1);
        await unitOfWork.CommitAsync();

        // Verify via a fresh DbContext rather than the repository under test,
        // so the assertion cannot be served from the repository's change tracker.
        await using var rootDbContext = CreateRootDbContext();
        var result = await rootDbContext.Set<TestEntity>().FindAsync(1);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_by_id_should_throw_EntityNotFoundException_when_entity_does_not_exist()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();

        var act = async () => await repository.DeleteAsync(999);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_by_entity_should_remove_entity()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        var entity = new TestEntity { Id = 1, Name = "ToDelete" };
        await repository.AddAsync(entity);
        await unitOfWork.CommitAsync();

        await repository.DeleteAsync(entity);
        await unitOfWork.CommitAsync();

        await using var rootDbContext = CreateRootDbContext();
        var result = await rootDbContext.Set<TestEntity>().FindAsync(entity.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_should_throw_EntityNotFoundException_when_entity_does_not_exist()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        var entity = new TestEntity { Id = 999, Name = "NonExistent" };

        var act = async () => await repository.UpdateAsync(entity);

        await act.Should().ThrowAsync<EntityNotFoundException>().Where(e => e.Message.Contains("not found"));
    }

    [Fact]
    public async Task UpdateAsync_should_update_entity()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "Original" });
        await unitOfWork.CommitAsync();

        var updatedEntity = new TestEntity { Id = 1, Name = "Updated" };
        await repository.UpdateAsync(updatedEntity);
        await unitOfWork.CommitAsync();

        await using var rootDbContext = CreateRootDbContext();
        var result = await rootDbContext.Set<TestEntity>().FindAsync(1);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task AddRangeAsync_should_add_multiple_entities()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        var entities = new List<TestEntity> { new() { Id = 1, Name = "First" }, new() { Id = 2, Name = "Second" }, new() { Id = 3, Name = "Third" } };

        var result = await repository.AddRangeAsync(entities);
        await unitOfWork.CommitAsync();

        result.Should().HaveCount(3);
        var all = await repository.GetAllAsync();
        all.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByIdAsync_with_onDbSet_should_return_entity()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "WithOnDbSet" });
        await unitOfWork.CommitAsync();

        var result = await repository.GetByIdAsync(1, q => q.Where(e => e.Name.Contains("WithOnDbSet")));

        result.Should().NotBeNull();
        result!.Name.Should().Be("WithOnDbSet");
    }

    [Fact]
    public async Task GetByIdAsync_with_onDbSet_should_return_null_when_filter_excludes_entity()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "Excluded" });
        await unitOfWork.CommitAsync();

        var result = await repository.GetByIdAsync(1, q => q.Where(e => e.Name == "NonExistent"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_with_keyValues_should_return_entity()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "KeyValueFind" });
        await unitOfWork.CommitAsync();

        var readRepo = CreateReadRepositoryAsync<TestEntity, int>();
        var result = await readRepo.GetByIdAsync([ 1 ]);

        result.Should().NotBeNull();
        result!.Name.Should().Be("KeyValueFind");
    }

    [Fact]
    public async Task GetAllAsync_with_query_filter_should_return_filtered_entities()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "Alpha" });
        await repository.AddAsync(new() { Id = 2, Name = "Beta" });
        await repository.AddAsync(new() { Id = 3, Name = "AlphaTwo" });
        await unitOfWork.CommitAsync();

        var result = await repository.GetAllAsync(e => e.Name.Contains("Alpha"));

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_with_onDbSet_should_apply_custom_query()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "First" });
        await repository.AddAsync(new() { Id = 2, Name = "Second" });
        await unitOfWork.CommitAsync();

        var result = await repository.GetAllAsync(onDbSet: q => q.OrderByDescending(e => e.Name));

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Second");
    }

    [Fact]
    public async Task FindFirstAsync_should_return_first_matching_entity()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "Alpha" });
        await repository.AddAsync(new() { Id = 2, Name = "Beta" });
        await unitOfWork.CommitAsync();

        var result = await repository.FindFirstAsync(e => e.Name == "Beta");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Beta");
    }

    [Fact]
    public async Task FindFirstAsync_with_onDbSet_should_apply_custom_query()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "Alpha" });
        await repository.AddAsync(new() { Id = 2, Name = "Beta" });
        await unitOfWork.CommitAsync();

        // Predicate matches multiple rows so the onDbSet ordering is observable:
        // both "Alpha" and "Beta" contain 'a', OrderByDescending(Name) should yield "Beta" first.
        var result = await repository.FindFirstAsync(e => e.Name.Contains('a'), q => q.OrderByDescending(e => e.Name));

        result.Should().NotBeNull();
        result!.Name.Should().Be("Beta");
    }

    [Fact]
    public async Task FindFirstAsync_should_return_null_when_no_match()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "Alpha" });
        await unitOfWork.CommitAsync();

        var result = await repository.FindFirstAsync(e => e.Name == "NonExistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task CountAsync_with_filter_should_return_filtered_count()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "Alpha" });
        await repository.AddAsync(new() { Id = 2, Name = "Beta" });
        await repository.AddAsync(new() { Id = 3, Name = "AlphaTwo" });
        await unitOfWork.CommitAsync();

        var readRepo = CreateReadRepositoryAsync<TestEntity, int>();
        var count = await readRepo.CountAsync(e => e.Name.Contains("Alpha"));

        count.Should().Be(2);
    }

    [Fact]
    public async Task CountAsync_without_filter_should_return_total_count()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new() { Id = 1, Name = "Alpha" });
        await repository.AddAsync(new() { Id = 2, Name = "Beta" });
        await unitOfWork.CommitAsync();

        var readRepo = CreateReadRepositoryAsync<TestEntity, int>();
        var count = await readRepo.CountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetPageAsync_should_return_paged_results()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        for (var i = 1; i <= 10; i++)
        {
            await repository.AddAsync(new() { Id = i, Name = $"Entity{i}" });
        }

        await unitOfWork.CommitAsync();

        // Explicit sort on Id so the page contents are deterministic — without a sort,
        // a regression that always returns the first three rows would still pass.
        var readRepo = CreateReadRepositoryAsync<TestEntity, int>();
        var page = await readRepo.GetPageAsync(2, 3, e => e.Id);

        page.Should().HaveCount(3);
        page.Select(e => e.Id).Should().Equal(4, 5, 6);
    }

    [Fact]
    public async Task GetPageAsync_with_sort_and_query_should_return_filtered_sorted_results()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        for (var i = 1; i <= 10; i++)
        {
            await repository.AddAsync(new() { Id = i, Name = $"Entity{i}" });
        }

        await unitOfWork.CommitAsync();

        var readRepo = CreateReadRepositoryAsync<TestEntity, int>();
        var page = await readRepo.GetPageAsync(1, 10, e => e.Name, e => e.Id > 7);

        page.Should().HaveCount(3);
        page.Should().OnlyContain(e => e.Id > 7);
        page.Select(e => e.Name).Should().BeInAscendingOrder();
    }
}
