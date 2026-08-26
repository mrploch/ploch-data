using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class RepositoryConstructionTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public async Task ReadRepositoryAsync_should_read_entities_when_constructed_with_only_a_DbContext()
    {
        // Arrange — seed via the plain DbContext; the read repository is the code under test (#111).
        var blog = new Blog { Name = "Read without an audit handler" };
        DbContext.Blogs.Add(blog);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act — construct the read repository directly; since #111 it requires no IAuditEntityHandler,
        // matching the synchronous ReadRepository<TEntity>.
        var repository = new ReadRepositoryAsync<Blog, int>(DbContext);

        // Assert — the full read surface works without an audit handler.
        (await repository.GetAllAsync()).Should().ContainSingle();
        (await repository.GetByIdAsync(blog.Id)).Should().NotBeNull();
        (await repository.FindFirstAsync(b => b.Name == blog.Name)).Should().NotBeNull();
        (await repository.CountAsync()).Should().Be(1);
    }

    [Fact]
    public void ReadRepositoryAsync_constructor_should_throw_when_db_context_is_null()
    {
        // The DbContext guard lives in the QueryableRepository base; this pins it for the new
        // single-argument constructor surface.
        var act = () => new ReadRepositoryAsync<Blog, int>(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("dbContext");
    }

    [Fact]
    public void ReadWriteRepositoryAsync_constructor_should_throw_when_audit_entity_handler_is_null()
    {
        // The null guard used to live in the ReadRepositoryAsync base constructor; #111 removed the
        // base dependency, so the write repository must fail fast on its own.
        var act = () => new ReadWriteRepositoryAsync<Blog, int>(DbContext, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("auditEntityHandler");
    }
}
