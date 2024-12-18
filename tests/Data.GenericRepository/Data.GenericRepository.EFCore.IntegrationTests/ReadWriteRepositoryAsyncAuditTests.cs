using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Data;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadWriteRepositoryAsyncAuditTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public async Task Update_and_CommitAsync_should_set_modified_time_audit_property()
    {
        var unitOfWork = CreateUnitOfWork();
        var (blog, _, _) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());
        await unitOfWork.CommitAsync();

        blog.ModifiedTime.Should().BeNull();

        blog.Name = "New Name";
        var modifiedTime = DateTimeOffset.UtcNow;
        await unitOfWork.Repository<Blog, int>().UpdateAsync(blog);
        await unitOfWork.CommitAsync();

        var updateBlog = await unitOfWork.Repository<Blog, int>().GetByIdAsync(blog.Id);
        updateBlog.ModifiedTime.Should().NotBeNull().And.BeAfter(modifiedTime);
    }

    [Fact]
    public async Task Add_and_CommitAsync_should_update_created_time_audit_property()
    {
        var unitOfWork = CreateUnitOfWork();
        var createdTime = DateTimeOffset.UtcNow;
        var (blog, _, _) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());
        await unitOfWork.CommitAsync();

        var createdBlog = await unitOfWork.Repository<Blog, int>().GetByIdAsync(blog.Id);
        createdBlog.CreatedTime.Should().NotBeNull().And.BeAfter(createdTime);
    }
}
