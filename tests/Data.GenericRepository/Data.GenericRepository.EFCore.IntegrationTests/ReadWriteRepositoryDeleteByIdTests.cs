using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadWriteRepositoryDeleteByIdTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public async Task Delete_by_id_should_remove_entity()
    {
        const int idToDelete = 10;

        // Arrange — seed via a separate context so the entity is not tracked by the unit of
        // work that performs the delete; the delete then genuinely round-trips through the database.
        await using (var seedContext = CreateRootDbContext())
        {
            await seedContext.TestEntities.AddAsync(new() { Id = idToDelete, Name = "ToDelete" });
            await seedContext.SaveChangesAsync();
        }

        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<TestEntity, int>();
        await repository.DeleteAsync(idToDelete);

        // After committing, it should be gone from the database.
        await unitOfWork.CommitAsync();

        await using var anotherDbContext = CreateRootDbContext();
        var result = await anotherDbContext.TestEntities.FindAsync(idToDelete);
        result.Should().BeNull();
    }

    [Fact]
    public void Delete_by_id_should_throw_EntityNotFoundException_when_entity_does_not_exist()
    {
        const int nonExistingId = 999;
        var repository = CreateReadWriteRepository<TestEntity, int>();

        var act = () => repository.Delete(nonExistingId);

        act.Should().Throw<EntityNotFoundException>().Where(e => e.Message.Contains(nonExistingId.ToString(CultureInfo.InvariantCulture)));
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
        // Arrange — seed via a separate context so the read below re-hydrates from the database
        // rather than being served the seeded entities from the read context's change tracker.
        var (blog, blogPost1, _) = await RepositoryHelper.AddTestBlogEntitiesAsync(CreateRootDbContext);

        var repository = CreateReadRepository<Blog, int>();
        var result = repository.GetById(blog.Id, q => q.Include(q => q.BlogPosts).ThenInclude(bp => bp.Tags).Include(q => q.BlogPosts).ThenInclude(bp => bp.Categories));

        await using var rootDbContext = CreateRootDbContext();

        var resultFromDb = await rootDbContext.Blogs.Include(q => q.BlogPosts)
                                              .ThenInclude(bp => bp.Tags)
                                              .Include(q => q.BlogPosts)
                                              .ThenInclude(bp => bp.Categories)
                                              .FirstAsync(b => b.Id == blog.Id);
        resultFromDb.Should().BeEquivalentTo(result, options => options.WithEntityEquivalencyOptions());
        result.Should().NotBeNull();
        result!.Id.Should().Be(blog.Id);
        result.Name.Should().Be(blog.Name);
        result.BlogPosts.Should().HaveCount(blog.BlogPosts.Count);

        // Verify eager-loading: Tags and Categories were included in the onDbSet query
        var loadedPost1 = result.BlogPosts.Single(p => p.Name == blogPost1.Name);
        loadedPost1.Tags.Should().HaveCount(blogPost1.Tags.Count);
        loadedPost1.Categories.Should().HaveCount(blogPost1.Categories.Count);
    }

    [Fact]
    public async Task GetById_with_onDbSet_should_return_null_when_filter_excludes_entity()
    {
        // Arrange — seed via a separate context so the read genuinely queries the database.
        await using (var seedContext = CreateRootDbContext())
        {
            await seedContext.TestEntities.AddAsync(new() { Id = 1, Name = "Excluded" });
            await seedContext.SaveChangesAsync();
        }

        var repository = CreateReadRepository<TestEntity, int>();
        var result = repository.GetById(1, q => q.Where(e => e.Name == "NonExistent"));

        result.Should().BeNull();
    }
}
