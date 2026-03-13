using Microsoft.EntityFrameworkCore;
using Ploch.Data.GenericRepository;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.SampleApp.Data;
using Ploch.Data.SampleApp.Model;

namespace Ploch.Data.SampleApp.IntegrationTests;

public class ArticleRepositoryTests : GenericRepositoryDataIntegrationTest<SampleAppDbContext>
{
    [Fact]
    public async Task AddAsync_should_persist_article_with_audit_properties()
    {
        var repository = CreateReadWriteRepositoryAsync<Article, int>();

        var article = new Article
        {
            Title = "Test Article",
            Description = "A test article",
            Contents = "Some content"
        };

        await repository.AddAsync(article);
        await DbContext.SaveChangesAsync();

        var saved = await repository.GetByIdAsync(article.Id);

        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Test Article");
        saved.CreatedTime.Should().NotBeNull();
        saved.ModifiedTime.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_should_persist_article_with_categories_and_tags()
    {
        var articleRepo = CreateReadWriteRepositoryAsync<Article, int>();
        var categoryRepo = CreateReadWriteRepositoryAsync<ArticleCategory, int>();
        var tagRepo = CreateReadWriteRepositoryAsync<ArticleTag, int>();

        var category = new ArticleCategory { Name = "Test Category" };
        await categoryRepo.AddAsync(category);

        var tag = new ArticleTag { Name = "Test Tag", Description = "A test tag" };
        await tagRepo.AddAsync(tag);

        var article = new Article
        {
            Title = "Test Article",
            Categories = new List<ArticleCategory> { category },
            Tags = new List<ArticleTag> { tag }
        };

        await articleRepo.AddAsync(article);
        await DbContext.SaveChangesAsync();

        var saved = await articleRepo.GetByIdAsync(
            article.Id,
            onDbSet: q => q.Include(a => a.Categories).Include(a => a.Tags));

        saved.Should().NotBeNull();
        saved!.Categories.Should().HaveCount(1);
        saved.Tags.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddAsync_should_persist_hierarchical_categories()
    {
        var categoryRepo = CreateReadWriteRepositoryAsync<ArticleCategory, int>();

        var grandchild = new ArticleCategory { Name = "Grandchild" };
        var child = new ArticleCategory
        {
            Name = "Child",
            Children = new List<ArticleCategory> { grandchild }
        };
        var parent = new ArticleCategory
        {
            Name = "Parent",
            Children = new List<ArticleCategory> { child }
        };

        await categoryRepo.AddAsync(parent);
        await DbContext.SaveChangesAsync();

        var savedParent = await categoryRepo.GetByIdAsync(
            parent.Id,
            onDbSet: q => q.Include(c => c.Children!));

        savedParent.Should().NotBeNull();
        savedParent!.Children.Should().HaveCount(1);
        savedParent.Children!.First().Name.Should().Be("Child");
    }

    [Fact]
    public async Task AddAsync_should_persist_article_with_properties()
    {
        var articleRepo = CreateReadWriteRepositoryAsync<Article, int>();

        var article = new Article
        {
            Title = "Article with Properties",
            Properties = new List<ArticleProperty>
            {
                new() { Name = "ReadingTime", Value = "5 minutes" },
                new() { Name = "Difficulty", Value = "Easy" }
            }
        };

        await articleRepo.AddAsync(article);
        await DbContext.SaveChangesAsync();

        var saved = await articleRepo.GetByIdAsync(
            article.Id,
            onDbSet: q => q.Include(a => a.Properties));

        saved.Should().NotBeNull();
        saved!.Properties.Should().HaveCount(2);
        saved.Properties.Should().Contain(p => p.Name == "ReadingTime" && p.Value == "5 minutes");
    }

    [Fact]
    public async Task GetPageAsync_should_return_paginated_results()
    {
        var repository = CreateReadWriteRepositoryAsync<Article, int>();

        for (var i = 0; i < 10; i++)
        {
            await repository.AddAsync(new Article { Title = $"Article {i + 1}" });
        }

        await DbContext.SaveChangesAsync();

        var page = await repository.GetPageAsync(1, 5);

        page.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetAllAsync_with_predicate_should_filter_results()
    {
        var repository = CreateReadWriteRepositoryAsync<Article, int>();

        await repository.AddAsync(new Article { Title = "C# Tutorial" });
        await repository.AddAsync(new Article { Title = "Java Guide" });
        await repository.AddAsync(new Article { Title = "C# Advanced" });
        await DbContext.SaveChangesAsync();

        var csharpArticles = await repository.GetAllAsync(a => a.Title.Contains("C#"));

        csharpArticles.Should().HaveCount(2);
    }
}
