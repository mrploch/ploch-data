using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.SampleApp.ConsoleApp.Commands;
using Ploch.Data.SampleApp.ConsoleApp.Services;
using Ploch.Data.SampleApp.Data;
using Ploch.Data.SampleApp.Model;

namespace Ploch.Data.SampleApp.IntegrationTests;

/// <summary>
///     End-to-end tests for every command exposed by the sample console application.
/// </summary>
public class SampleAppCommandsTests : GenericRepositoryDataIntegrationTest<SampleAppDbContext>
{
    private IServiceScopeFactory ScopeFactory => RootServiceProvider.GetRequiredService<IServiceScopeFactory>();

    [Fact]
    public async Task SeedCommand_should_populate_the_database_with_the_sample_data_set()
    {
        var exitCode = await new SeedCommand(ScopeFactory).ExecuteAsync(new SeedCommand.Settings { Keep = true, FillerArticles = 3 });

        exitCode.Should().Be(0);

        var dbContext = CreateRootDbContext();
        (await dbContext.Authors.CountAsync()).Should().Be(1);
        (await dbContext.Articles.CountAsync()).Should().Be(5);
        (await dbContext.ArticleTags.CountAsync()).Should().Be(3);
        (await dbContext.ArticleCategories.CountAsync()).Should().Be(4);
        (await dbContext.ArticleProperties.CountAsync()).Should().Be(2);
    }

    [Fact]
    public void SeedCommand_should_reject_a_negative_filler_article_count()
    {
        var result = new SeedCommand.Settings { FillerArticles = -1 }.Validate();

        result.Successful.Should().BeFalse();
    }

    [Fact]
    public async Task ListArticlesCommand_should_succeed_for_every_page()
    {
        await SeedAsync();

        var exitCode = await new ListArticlesCommand(ScopeFactory).ExecuteAsync(new ListArticlesCommand.Settings { AllPages = true, PageSize = 2 });

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task ListArticlesCommand_should_succeed_on_an_empty_database()
    {
        var exitCode = await new ListArticlesCommand(ScopeFactory).ExecuteAsync(new ListArticlesCommand.Settings());

        exitCode.Should().Be(0);
    }

    [Fact]
    public void ListArticlesCommand_should_reject_a_page_number_below_one()
    {
        var result = new ListArticlesCommand.Settings { Page = 0 }.Validate();

        result.Successful.Should().BeFalse();
    }

    [Fact]
    public async Task ShowArticleCommand_should_succeed_for_an_existing_article()
    {
        var articleId = (await SeedAsync()).FeaturedArticle.Id;

        var exitCode = await new ShowArticleCommand(ScopeFactory).ExecuteAsync(new ShowArticleCommand.Settings { Id = articleId });

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task ShowArticleCommand_should_fail_for_an_unknown_article()
    {
        await SeedAsync();

        var exitCode = await new ShowArticleCommand(ScopeFactory).ExecuteAsync(new ShowArticleCommand.Settings { Id = 9999 });

        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task SearchArticlesCommand_should_succeed_for_a_matching_title_fragment()
    {
        await SeedAsync();

        var exitCode = await new SearchArticlesCommand(ScopeFactory).ExecuteAsync(new SearchArticlesCommand.Settings { Text = "Entity Framework" });

        exitCode.Should().Be(0);
    }

    [Fact]
    public void SearchArticlesCommand_should_reject_empty_search_text()
    {
        var result = new SearchArticlesCommand.Settings { Text = "  " }.Validate();

        result.Successful.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateArticleCommand_should_rename_the_article_and_refresh_the_modified_time()
    {
        var seedResult = await SeedAsync();
        var articleId = seedResult.FeaturedArticle.Id;
        var originalModifiedTime = seedResult.FeaturedArticle.ModifiedTime;

        await Task.Delay(50);

        var exitCode = await new UpdateArticleCommand(ScopeFactory).ExecuteAsync(new UpdateArticleCommand.Settings
                                                                                {
                                                                                    Id = articleId, Title = "A brand new title"
                                                                                });

        exitCode.Should().Be(0);

        var dbContext = CreateRootDbContext();
        var article = await dbContext.Articles.SingleAsync(a => a.Id == articleId);
        article.Title.Should().Be("A brand new title");
        article.ModifiedTime.Should().NotBeNull();
        article.ModifiedTime.Should().BeAfter(originalModifiedTime!.Value);
    }

    [Fact]
    public async Task UpdateArticleCommand_should_fail_for_an_unknown_article()
    {
        var exitCode = await new UpdateArticleCommand(ScopeFactory).ExecuteAsync(new UpdateArticleCommand.Settings { Id = 9999, Title = "Nope" });

        exitCode.Should().Be(1);
    }

    [Fact]
    public void UpdateArticleCommand_should_reject_an_empty_title()
    {
        var result = new UpdateArticleCommand.Settings { Id = 1, Title = string.Empty }.Validate();

        result.Successful.Should().BeFalse();
    }

    [Fact]
    public async Task ListAuthorsCommand_should_succeed_after_seeding()
    {
        await SeedAsync();

        var exitCode = await new ListAuthorsCommand(ScopeFactory).ExecuteAsync(new ListAuthorsCommand.Settings());

        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task DemoCommand_should_run_the_whole_walkthrough_and_leave_the_expected_data_behind()
    {
        var exitCode = await new DemoCommand(ScopeFactory).ExecuteAsync(new DemoCommand.Settings { AuthorName = "Ada Lovelace", FillerArticles = 4 });

        exitCode.Should().Be(0);

        var dbContext = CreateRootDbContext();
        (await dbContext.Authors.SingleAsync()).Name.Should().Be("Ada Lovelace");
        (await dbContext.Articles.CountAsync()).Should().Be(6);
        (await dbContext.Articles.AnyAsync(article => article.Title.EndsWith("(Updated)"))).Should().BeTrue();
    }

    /// <summary>
    ///     Configures the services the sample commands resolve from their scope.
    /// </summary>
    /// <param name="services">The service collection.</param>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddScoped<SampleDataSeeder>();
    }

    private async Task<SeedResult> SeedAsync()
    {
        using var scope = ScopeFactory.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<SampleDataSeeder>();

        return await seeder.SeedAsync("Jane Smith", 2);
    }
}
