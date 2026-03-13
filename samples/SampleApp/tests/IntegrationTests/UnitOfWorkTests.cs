using Ploch.Data.GenericRepository;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.SampleApp.Data;
using Ploch.Data.SampleApp.Model;

namespace Ploch.Data.SampleApp.IntegrationTests;

public class UnitOfWorkTests : GenericRepositoryDataIntegrationTest<SampleAppDbContext>
{
    [Fact]
    public async Task CommitAsync_should_persist_changes_across_multiple_repositories()
    {
        var unitOfWork = CreateUnitOfWork();

        var authorRepo = unitOfWork.Repository<Author, int>();
        var articleRepo = unitOfWork.Repository<Article, int>();

        var author = new Author { Name = "Test Author" };
        await authorRepo.AddAsync(author);

        var article = new Article { Title = "Test Article", Author = author };
        await articleRepo.AddAsync(article);

        await unitOfWork.CommitAsync();

        var savedAuthor = await authorRepo.GetByIdAsync(author.Id);
        var savedArticle = await articleRepo.GetByIdAsync(article.Id);

        savedAuthor.Should().NotBeNull();
        savedArticle.Should().NotBeNull();
        savedArticle!.AuthorId.Should().Be(savedAuthor!.Id);
    }

    [Fact]
    public async Task CommitAsync_should_update_audit_modified_time_on_update()
    {
        var unitOfWork = CreateUnitOfWork();
        var articleRepo = unitOfWork.Repository<Article, int>();

        var article = new Article { Title = "Original Title" };
        await articleRepo.AddAsync(article);
        await unitOfWork.CommitAsync();

        var createdTime = article.CreatedTime;
        createdTime.Should().NotBeNull();

        await Task.Delay(50);

        article.Title = "Updated Title";
        await articleRepo.UpdateAsync(article);
        await unitOfWork.CommitAsync();

        article.ModifiedTime.Should().NotBeNull();
        article.ModifiedTime.Should().BeAfter(createdTime!.Value);
    }

    [Fact]
    public async Task DeleteAsync_should_remove_entity()
    {
        var unitOfWork = CreateUnitOfWork();
        var tagRepo = unitOfWork.Repository<ArticleTag, int>();

        var tag = new ArticleTag { Name = "Temporary Tag" };
        await tagRepo.AddAsync(tag);
        await unitOfWork.CommitAsync();

        var tagId = tag.Id;

        await tagRepo.DeleteAsync(tag);
        await unitOfWork.CommitAsync();

        var deleted = await tagRepo.GetByIdAsync(tagId);
        deleted.Should().BeNull();
    }
}
