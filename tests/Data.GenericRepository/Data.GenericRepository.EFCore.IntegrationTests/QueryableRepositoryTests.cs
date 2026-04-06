using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class QueryableRepositoryTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public async Task Entities_should_return_queryable_of_all_entities()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new TestEntity { Id = 1, Name = "First" });
        await repository.AddAsync(new TestEntity { Id = 2, Name = "Second" });
        await unitOfWork.CommitAsync();

        var queryableRepo = (IQueryableRepository<TestEntity>)CreateReadRepositoryAsync<TestEntity, int>();
        var entities = queryableRepo.Entities;

        entities.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPageQuery_should_return_paged_queryable()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        for (var i = 1; i <= 15; i++)
        {
            await repository.AddAsync(new TestEntity { Id = i, Name = $"Entity{i:D2}" });
        }

        await unitOfWork.CommitAsync();

        var queryableRepo = (IQueryableRepository<TestEntity>)CreateReadRepositoryAsync<TestEntity, int>();
        var pageQuery = queryableRepo.GetPageQuery(2, 5);

        var result = pageQuery.ToList();
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPageQuery_with_sort_should_order_results()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.AddAsync(new TestEntity { Id = 1, Name = "Charlie" });
        await repository.AddAsync(new TestEntity { Id = 2, Name = "Alpha" });
        await repository.AddAsync(new TestEntity { Id = 3, Name = "Bravo" });
        await unitOfWork.CommitAsync();

        var queryableRepo = (IQueryableRepository<TestEntity>)CreateReadRepositoryAsync<TestEntity, int>();
        var pageQuery = queryableRepo.GetPageQuery(1, 3, sortBy: e => e.Name);

        var result = pageQuery.ToList();
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alpha");
        result[1].Name.Should().Be("Bravo");
        result[2].Name.Should().Be("Charlie");
    }

    [Fact]
    public async Task GetPageQuery_with_query_filter_should_filter_results()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        for (var i = 1; i <= 10; i++)
        {
            await repository.AddAsync(new TestEntity { Id = i, Name = $"Entity{i}" });
        }

        await unitOfWork.CommitAsync();

        var queryableRepo = (IQueryableRepository<TestEntity>)CreateReadRepositoryAsync<TestEntity, int>();
        var pageQuery = queryableRepo.GetPageQuery(1, 10, query: e => e.Id > 5);

        var result = pageQuery.ToList();
        result.Should().HaveCount(5);
        result.Should().OnlyContain(e => e.Id > 5);
    }

    [Fact]
    public async Task GetPageQuery_with_onDbSet_should_apply_custom_query()
    {
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        for (var i = 1; i <= 10; i++)
        {
            await repository.AddAsync(new TestEntity { Id = i, Name = $"Entity{i}" });
        }

        await unitOfWork.CommitAsync();

        var queryableRepo = (IQueryableRepository<TestEntity>)CreateReadRepositoryAsync<TestEntity, int>();

        // Request all 10 items in one page, but the onDbSet filter limits to IDs <= 3
        var pageQuery = queryableRepo.GetPageQuery(1, 10, onDbSet: q => q.Where(e => e.Id <= 3));

        var result = pageQuery.ToList();
        result.Should().HaveCount(3);
        result.Should().OnlyContain(e => e.Id <= 3);
    }

    [Fact]
    public void GetPageQuery_should_throw_when_page_number_is_zero()
    {
        var queryableRepo = (IQueryableRepository<TestEntity>)CreateReadRepositoryAsync<TestEntity, int>();

        var act = () => queryableRepo.GetPageQuery(0, 5);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetPageQuery_should_throw_when_page_size_is_zero()
    {
        var queryableRepo = (IQueryableRepository<TestEntity>)CreateReadRepositoryAsync<TestEntity, int>();

        var act = () => queryableRepo.GetPageQuery(1, 0);

        act.Should().Throw<ArgumentException>();
    }
}
