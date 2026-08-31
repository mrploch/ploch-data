using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository;
using Ploch.Data.SampleApp.ConsoleApp.Services;
using Ploch.Data.SampleApp.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ploch.Data.SampleApp.ConsoleApp.Commands;

/// <summary>
///     Runs the complete guided walkthrough: seeding, eager loading, updating, pagination, filtering, and
///     direct repository injection — the end-to-end tour every other command exposes one step of.
/// </summary>
/// <param name="scopeFactory">The factory used to open a dependency-injection scope for the command.</param>
public class DemoCommand(IServiceScopeFactory scopeFactory) : SampleAppCommand<DemoCommand.Settings>(scopeFactory)
{
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(IServiceProvider services, Settings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.Write(new Rule("[green]Ploch.Data Sample Application[/]").LeftJustified());

        var seeder = services.GetRequiredService<SampleDataSeeder>();
        await seeder.ResetDatabaseAsync(cancellationToken);

        AnsiConsole.Write(new Rule("Seeding authors, categories, tags and articles").LeftJustified());
        var seedResult = await seeder.SeedAsync(settings.AuthorName, settings.FillerArticles, cancellationToken);
        AnsiConsole.MarkupLine($"Created author [green]{seedResult.Author.Name.EscapeMarkup()}[/] (Id: {seedResult.Author.Id}).");
        AnsiConsole.MarkupLine($"  CreatedTime automatically set: [blue]{seedResult.Author.CreatedTime}[/]");
        AnsiConsole.MarkupLine("Created category hierarchy [green]Technology > .NET > Entity Framework Core[/] and standalone [green]Science[/].");
        AnsiConsole.MarkupLine($"Created [blue]{seedResult.TagCount}[/] tags and [blue]{seedResult.ArticleCount}[/] articles.");

        var articleId = seedResult.FeaturedArticle.Id;

        AnsiConsole.Write(new Rule("Reading an article with eager loading").LeftJustified());
        var readRepository = services.GetRequiredService<IReadRepositoryAsync<Article, int>>();
        var loadedArticle = await readRepository.GetByIdAsync(articleId,
                                                              onDbSet: query => query.Include(a => a.Author)
                                                                                     .Include(a => a.Categories)
                                                                                     .Include(a => a.Tags)
                                                                                     .Include(a => a.Properties),
                                                              cancellationToken);

        AnsiConsole.MarkupLine($"Loaded article: [green]{loadedArticle!.Title.EscapeMarkup()}[/]");
        AnsiConsole.MarkupLine($"  Author: {(loadedArticle.Author?.Name ?? "(none)").EscapeMarkup()}");
        AnsiConsole.MarkupLine($"  Categories: {string.Join(", ", loadedArticle.Categories?.Select(c => c.Name) ?? []).EscapeMarkup()}");
        AnsiConsole.MarkupLine($"  Tags: {string.Join(", ", loadedArticle.Tags.Select(t => t.Name)).EscapeMarkup()}");
        AnsiConsole.MarkupLine($"  Properties: {string.Join(", ", loadedArticle.Properties.Select(p => $"{p.Name}={p.Value}")).EscapeMarkup()}");

        AnsiConsole.Write(new Rule("Filtered queries").LeftJustified());
        var matches = await readRepository.GetAllAsync(onDbSet: query => query.Where(article => article.Title.Contains("Entity Framework")), cancellationToken: cancellationToken);
        AnsiConsole.MarkupLine($"Articles about Entity Framework: [blue]{matches.Count}[/]");
        foreach (var article in matches)
        {
            AnsiConsole.MarkupLine($"  - {article.Title.EscapeMarkup()}");
        }

        AnsiConsole.Write(new Rule("Updating an article").LeftJustified());
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var articleRepository = unitOfWork.Repository<Article, int>();
        var articleToUpdate = await articleRepository.GetByIdAsync(articleId, cancellationToken: cancellationToken);
        var originalModifiedTime = articleToUpdate!.ModifiedTime;
        await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
        articleToUpdate.Title = $"{articleToUpdate.Title} (Updated)";
        await articleRepository.UpdateAsync(articleToUpdate, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        AnsiConsole.MarkupLine($"Updated article title to: [green]{articleToUpdate.Title.EscapeMarkup()}[/]");
        AnsiConsole.MarkupLine($"  ModifiedTime changed from [blue]{originalModifiedTime}[/] to [blue]{articleToUpdate.ModifiedTime}[/]");

        AnsiConsole.Write(new Rule("Pagination").LeftJustified());
        var totalCount = await readRepository.CountAsync(cancellationToken: cancellationToken);
        AnsiConsole.MarkupLine($"Total articles: [blue]{totalCount}[/]");
        var page1 = await readRepository.GetPageAsync(1, 5, sortBy: article => article.Id, cancellationToken: cancellationToken);
        AnsiConsole.MarkupLine($"Page 1 (5 per page): {string.Join(", ", page1.Select(a => a.Title)).EscapeMarkup()}");
        var page2 = await readRepository.GetPageAsync(2, 5, sortBy: article => article.Id, cancellationToken: cancellationToken);
        AnsiConsole.MarkupLine($"Page 2 (5 per page): {string.Join(", ", page2.Select(a => a.Title)).EscapeMarkup()}");

        AnsiConsole.Write(new Rule("Direct repository injection").LeftJustified());
        var authorRepository = services.GetRequiredService<IReadRepositoryAsync<Author, int>>();
        var authors = await authorRepository.GetAllAsync(cancellationToken: cancellationToken);
        AnsiConsole.MarkupLine($"Authors ([blue]{authors.Count}[/]):");
        foreach (var author in authors)
        {
            AnsiConsole.MarkupLine($"  - [green]{author.Name.EscapeMarkup()}[/]: {(author.Description ?? string.Empty).EscapeMarkup()}");
        }

        AnsiConsole.Write(new Rule("[green]Sample Application Complete[/]").LeftJustified());

        return 0;
    }

    /// <summary>
    ///     Options accepted by the <c>demo</c> command.
    /// </summary>
    public class Settings : CommandSettings
    {
        /// <summary>
        ///     Gets the name given to the seeded author.
        /// </summary>
        [CommandOption("-a|--author <NAME>")]
        [Description("Name of the author the seeded articles are attributed to.")]
        [DefaultValue("Jane Smith")]
        public string AuthorName { get; init; } = "Jane Smith";

        /// <summary>
        ///     Gets the number of additional filler articles created so that pagination has something to page over.
        /// </summary>
        [CommandOption("-f|--filler <COUNT>")]
        [Description("Number of additional filler articles to create so pagination is meaningful.")]
        [DefaultValue(10)]
        public int FillerArticles { get; init; } = 10;

        /// <inheritdoc />
        public override ValidationResult Validate() =>
            FillerArticles < 0 ? ValidationResult.Error("--filler must be zero or greater.") : ValidationResult.Success();
    }
}
