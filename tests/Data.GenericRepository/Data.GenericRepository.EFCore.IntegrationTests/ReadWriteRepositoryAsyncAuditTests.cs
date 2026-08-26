using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadWriteRepositoryAsyncAuditTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public async Task Reads_should_not_write_access_audit_properties()
    {
        // Arrange — seed via the plain DbContext with the access-audit properties explicitly unset.
        // The read path is the code under test, so it must not be used for seeding.
        var blog = new Blog { Name = "Never accessed" };
        DbContext.Blogs.Add(blog);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act — exercise both read paths that used to call IAuditEntityHandler.HandleAccess (#104).
        using var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<Blog, int>();

        (await repository.GetAllAsync()).Should().ContainSingle();
        (await repository.GetByIdAsync(blog.Id)).Should().NotBeNull();
        await unitOfWork.CommitAsync();

        // Assert — reads are not audited: nothing stamped the access properties, and nothing persisted.
        // Verified through a fresh context so a change-tracked in-memory value cannot mask a missing write.
        var rootDbContext = CreateRootDbContext();
        var persisted = await rootDbContext.Blogs.FindAsync(blog.Id);

        persisted.Should().NotBeNull();
        persisted.AccessedTime.Should().BeNull();
        persisted.LastAccessedBy.Should().BeNull();
    }

    [Fact]
    public async Task Update_and_CommitAsync_should_set_modified_time_audit_property()
    {
        var unitOfWork = CreateUnitOfWork();

        // Seeded through the repository on purpose: this class verifies the audit behaviour of the
        // repository write path, and the assertion below checks that a repository Add leaves
        // ModifiedTime unset. A separate-context seed would make that assertion vacuous.
        var (blog, _, _) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());
        await unitOfWork.CommitAsync();

        blog.ModifiedTime.Should().BeNull();

        blog.Name = "New Name";
        var modifiedTime = DateTimeOffset.UtcNow;
        await unitOfWork.Repository<Blog, int>().UpdateAsync(blog);
        await unitOfWork.CommitAsync();

        var updateBlog = await unitOfWork.Repository<Blog, int>().GetByIdAsync(blog.Id);
        updateBlog.Should().NotBeNull();
        updateBlog.ModifiedTime.Should().NotBeNull().And.BeAfter(modifiedTime);
    }

    [Fact]
    public async Task UpdateAsync_should_preserve_creation_audit_properties_on_partial_detached_update()
    {
        // Arrange — seed via the plain DbContext with known creation-audit values; the repository
        // write path is the code under test, so it must not be used for seeding.
        var createdTime = DateTimeOffset.UtcNow.AddDays(-1);
        var blog = new Blog { Name = "Original Name", CreatedTime = createdTime, CreatedBy = "original-creator" };
        DbContext.Blogs.Add(blog);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act — a partial detached entity supplies only Id and Name; every other property is at its default.
        var unitOfWork = CreateUnitOfWork();
        await unitOfWork.Repository<Blog, int>().UpdateAsync(new Blog { Id = blog.Id, Name = "Updated Name" });
        await unitOfWork.CommitAsync();

        // Assert — verify via a fresh DbContext so values are re-hydrated from the database.
        var rootDbContext = CreateRootDbContext();
        var updatedBlog = await rootDbContext.Set<Blog>().FindAsync(blog.Id);
        updatedBlog.Should().NotBeNull();
        updatedBlog!.Name.Should().Be("Updated Name");
        updatedBlog.CreatedTime.Should().NotBeNull("a partial detached update must not blank the creation timestamp");
        updatedBlog.CreatedTime!.Value.Should().BeCloseTo(createdTime, TimeSpan.FromMilliseconds(1));
        updatedBlog.CreatedBy.Should().Be("original-creator", "a partial detached update must not blank the creator");
        updatedBlog.ModifiedTime.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_should_ignore_caller_supplied_creation_audit_values()
    {
        // Arrange
        var createdTime = DateTimeOffset.UtcNow.AddDays(-2);
        var blog = new Blog { Name = "Original Name", CreatedTime = createdTime, CreatedBy = "original-creator" };
        DbContext.Blogs.Add(blog);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act — a fully-formed detached entity deliberately supplies different creation-audit values.
        var tamperedBlog = new Blog
        {
            Id = blog.Id,
            Name = "Updated Name",
            CreatedTime = DateTimeOffset.UtcNow,
            CreatedBy = "someone-else"
        };
        var unitOfWork = CreateUnitOfWork();
        await unitOfWork.Repository<Blog, int>().UpdateAsync(tamperedBlog);
        await unitOfWork.CommitAsync();

        // Assert — creation audit is immutable through the repository; the supplied values are ignored.
        var rootDbContext = CreateRootDbContext();
        var updatedBlog = await rootDbContext.Set<Blog>().FindAsync(blog.Id);
        updatedBlog.Should().NotBeNull();
        updatedBlog!.Name.Should().Be("Updated Name");
        updatedBlog.CreatedTime.Should().NotBeNull();
        updatedBlog.CreatedTime!.Value.Should().BeCloseTo(createdTime, TimeSpan.FromMilliseconds(1));
        updatedBlog.CreatedBy.Should().Be("original-creator");
    }

    [Fact]
    public async Task UpdateAsync_should_keep_null_creation_audit_values_when_caller_supplies_them()
    {
        // Arrange — an entity persisted without creation-audit values (e.g. imported data).
        var blog = new Blog { Name = "Original Name" };
        DbContext.Blogs.Add(blog);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act — the caller attempts to backfill creation-audit values through UpdateAsync.
        var updateBlog = new Blog
        {
            Id = blog.Id,
            Name = "Updated Name",
            CreatedTime = DateTimeOffset.UtcNow,
            CreatedBy = "backfiller"
        };
        var unitOfWork = CreateUnitOfWork();
        await unitOfWork.Repository<Blog, int>().UpdateAsync(updateBlog);
        await unitOfWork.CommitAsync();

        // Assert — creation audit stays exactly as persisted, including null.
        var rootDbContext = CreateRootDbContext();
        var updatedBlog = await rootDbContext.Set<Blog>().FindAsync(blog.Id);
        updatedBlog.Should().NotBeNull();
        updatedBlog!.Name.Should().Be("Updated Name");
        updatedBlog.CreatedTime.Should().BeNull();
        updatedBlog.CreatedBy.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_should_ignore_creation_audit_changes_on_tracked_entity()
    {
        // Arrange
        var createdTime = DateTimeOffset.UtcNow.AddDays(-3);
        var blog = new Blog { Name = "Original Name", CreatedTime = createdTime, CreatedBy = "original-creator" };
        DbContext.Blogs.Add(blog);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act — fetch-modify-update on the same tracked instance, mutating creation audit directly.
        var unitOfWork = CreateUnitOfWork();
        var repository = unitOfWork.Repository<Blog, int>();
        var trackedBlog = await repository.GetByIdAsync(blog.Id);
        trackedBlog.Should().NotBeNull();
        trackedBlog!.Name = "Updated Name";
        trackedBlog.CreatedTime = DateTimeOffset.UtcNow;
        trackedBlog.CreatedBy = "someone-else";
        await repository.UpdateAsync(trackedBlog);
        await unitOfWork.CommitAsync();

        // Assert — the direct mutation of creation audit is ignored; the persisted values are kept.
        var rootDbContext = CreateRootDbContext();
        var updatedBlog = await rootDbContext.Set<Blog>().FindAsync(blog.Id);
        updatedBlog.Should().NotBeNull();
        updatedBlog!.Name.Should().Be("Updated Name");
        updatedBlog.CreatedTime.Should().NotBeNull();
        updatedBlog.CreatedTime!.Value.Should().BeCloseTo(createdTime, TimeSpan.FromMilliseconds(1));
        updatedBlog.CreatedBy.Should().Be("original-creator");
    }

    [Fact]
    public async Task Add_and_CommitAsync_should_update_created_time_audit_property()
    {
        var unitOfWork = CreateUnitOfWork();
        var createdTime = DateTimeOffset.UtcNow;

        // Adding through the repository and committing IS the operation under test here, so this
        // seeding deliberately stays repository-based.
        var (blog, _, _) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());
        await unitOfWork.CommitAsync();

        var createdBlog = await unitOfWork.Repository<Blog, int>().GetByIdAsync(blog.Id);
        createdBlog.Should().NotBeNull();
        createdBlog.CreatedTime.Should().NotBeNull().And.BeAfter(createdTime);
    }
}
