using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.SampleApp.ConsoleApp.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ploch.Data.SampleApp.ConsoleApp.Commands;

/// <summary>
///     Creates the sample database and populates it with authors, categories, tags, and articles.
/// </summary>
/// <param name="scopeFactory">The factory used to open a dependency-injection scope for the command.</param>
public class SeedCommand(IServiceScopeFactory scopeFactory) : SampleAppCommand<SeedCommand.Settings>(scopeFactory)
{
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(IServiceProvider services, Settings settings, CancellationToken cancellationToken)
    {
        var seeder = services.GetRequiredService<SampleDataSeeder>();

        if (settings.Keep)
        {
            await seeder.EnsureDatabaseAsync(cancellationToken);
        }
        else
        {
            await seeder.ResetDatabaseAsync(cancellationToken);
            AnsiConsole.MarkupLine("[grey]Database dropped and recreated.[/]");
        }

        var result = await seeder.SeedAsync(settings.AuthorName, settings.FillerArticles, cancellationToken);

        AnsiConsole.MarkupLine($"Created author [green]{result.Author.Name.EscapeMarkup()}[/] (Id: {result.Author.Id}).");
        AnsiConsole.MarkupLine($"  Audit [grey]CreatedTime[/] set automatically to [blue]{result.Author.CreatedTime}[/].");
        AnsiConsole.MarkupLine("Created category hierarchy [green]Technology > .NET > Entity Framework Core[/] and standalone [green]Science[/].");
        AnsiConsole.MarkupLine($"Categories: [blue]{result.CategoryCount}[/], tags: [blue]{result.TagCount}[/], articles: [blue]{result.ArticleCount}[/].");
        AnsiConsole.MarkupLine($"Featured article [green]{result.FeaturedArticle.Title.EscapeMarkup()}[/] (Id: {result.FeaturedArticle.Id}) has "
                             + $"[blue]{result.FeaturedArticle.Categories?.Count ?? 0}[/] categories, "
                             + $"[blue]{result.FeaturedArticle.Tags.Count}[/] tags and "
                             + $"[blue]{result.FeaturedArticle.Properties.Count}[/] properties.");

        return 0;
    }

    /// <summary>
    ///     Options accepted by the <c>seed</c> command.
    /// </summary>
    public class Settings : SeedDataSettings
    {
        /// <summary>
        ///     Gets a value indicating whether existing data is kept instead of the database being recreated.
        /// </summary>
        [CommandOption("-k|--keep")]
        [Description("Keep the existing database instead of dropping and recreating it.")]
        public bool Keep { get; init; }
    }
}
