using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadWriteRepositoryAuditTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public void Update_should_preserve_creation_audit_properties_on_partial_detached_update()
    {
        // Arrange — seed via the plain DbContext with known creation-audit values; the repository
        // write path is the code under test, so it must not be used for seeding.
        var createdTime = DateTimeOffset.UtcNow.AddDays(-1);
        var blog = new Blog { Name = "Original Name", CreatedTime = createdTime, CreatedBy = "original-creator" };
        DbContext.Blogs.Add(blog);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();

        // Act — a partial detached entity supplies only Id and Name; every other property is at its default.
        var repository = CreateReadWriteRepository<Blog, int>();
        repository.Update(new Blog { Id = blog.Id, Name = "Updated Name" });
        DbContext.SaveChanges();

        // Assert — verify via a fresh DbContext so values are re-hydrated from the database.
        var rootDbContext = CreateRootDbContext();
        var updatedBlog = rootDbContext.Set<Blog>().Find(blog.Id);
        updatedBlog.Should().NotBeNull();
        updatedBlog!.Name.Should().Be("Updated Name");
        updatedBlog.CreatedTime.Should().NotBeNull("a partial detached update must not blank the creation timestamp");
        updatedBlog.CreatedTime!.Value.Should().BeCloseTo(createdTime, TimeSpan.FromMilliseconds(1));
        updatedBlog.CreatedBy.Should().Be("original-creator", "a partial detached update must not blank the creator");
    }
}
