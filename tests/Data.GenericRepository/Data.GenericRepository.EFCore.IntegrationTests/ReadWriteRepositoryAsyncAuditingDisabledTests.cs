using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadWriteRepositoryAsyncAuditingDisabledTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.Configure<RepositoriesConfiguration>(static configuration => configuration.EnableAuditing = false);
    }

    [Fact]
    public async Task UpdateAsync_should_apply_supplied_creation_audit_values_when_auditing_is_disabled()
    {
        // Arrange
        var originalCreatedTime = DateTimeOffset.UtcNow.AddDays(-1);
        var blog = new Blog { Name = "Original Name", CreatedTime = originalCreatedTime, CreatedBy = "original-creator" };
        DbContext.Blogs.Add(blog);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act — with auditing disabled, the library must stay neutral: a plain full-entity update
        // with no creation-audit protection, matching AuditEntityHandler's own EnableAuditing gate.
        var newCreatedTime = DateTimeOffset.UtcNow;
        var updateBlog = new Blog
        {
            Id = blog.Id,
            Name = "Updated Name",
            CreatedTime = newCreatedTime,
            CreatedBy = "new-creator"
        };
        var unitOfWork = CreateUnitOfWork();
        await unitOfWork.Repository<Blog, int>().UpdateAsync(updateBlog);
        await unitOfWork.CommitAsync();

        // Assert — supplied values are applied verbatim and ModifiedTime is not set.
        var rootDbContext = CreateRootDbContext();
        var updatedBlog = await rootDbContext.Set<Blog>().FindAsync(blog.Id);
        updatedBlog.Should().NotBeNull();
        updatedBlog!.Name.Should().Be("Updated Name");
        updatedBlog.CreatedTime.Should().NotBeNull();
        updatedBlog.CreatedTime!.Value.Should().BeCloseTo(newCreatedTime, TimeSpan.FromMilliseconds(1));
        updatedBlog.CreatedBy.Should().Be("new-creator");
        updatedBlog.ModifiedTime.Should().BeNull("auditing is disabled, so the modification timestamp is not set");
    }
}
