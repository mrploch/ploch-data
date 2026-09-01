using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository;
using Ploch.Data.SampleApp.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ploch.Data.SampleApp.ConsoleApp.Commands;

/// <summary>
///     Shows a single article with its author, categories, tags, and properties eagerly loaded.
/// </summary>
/// <param name="scopeFactory">The factory used to open a dependency-injection scope for the command.</param>
public class ShowArticleCommand(IServiceScopeFactory scopeFactory) : SampleAppCommand<ShowArticleCommand.Settings>(scopeFactory)
{
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(IServiceProvider services, Settings settings, CancellationToken cancellationToken)
    {
        var repository = services.GetRequiredService<IReadRepositoryAsync<Article, int>>();

        var article = await repository.GetByIdAsync(settings.Id,
                                                   // Three collection navigations in one query would multiply rows
                                                   // together (a Cartesian explosion), so the query is split.
                                                   onDbSet: query => query.Include(a => a.Author)
                                                                          .Include(a => a.Categories)
                                                                          .Include(a => a.Tags)
                                                                          .Include(a => a.Properties)
                                                                          .AsSplitQuery(),
                                                   cancellationToken);

        if (article is null)
        {
            AnsiConsole.MarkupLine($"[red]No article with Id {settings.Id} was found.[/]");

            return 1;
        }

        var table = new Table().Border(TableBorder.Rounded).AddColumn("Field").AddColumn("Value");
        table.AddRow("Id", article.Id.ToString());
        table.AddRow("Title", article.Title.EscapeMarkup());
        table.AddRow("Description", (article.Description ?? string.Empty).EscapeMarkup());
        table.AddRow("Contents", (article.Contents ?? string.Empty).EscapeMarkup());
        table.AddRow("Author", (article.Author?.Name ?? "(none)").EscapeMarkup());
        table.AddRow("Categories", string.Join(", ", article.Categories?.Select(category => category.Name) ?? []).EscapeMarkup());
        table.AddRow("Tags", string.Join(", ", article.Tags.Select(tag => tag.Name)).EscapeMarkup());
        table.AddRow("Properties", string.Join(", ", article.Properties.Select(property => $"{property.Name}={property.Value}")).EscapeMarkup());
        table.AddRow("CreatedTime", article.CreatedTime?.ToString() ?? "(null)");
        table.AddRow("ModifiedTime", article.ModifiedTime?.ToString() ?? "(null)");

        AnsiConsole.Write(table);

        return 0;
    }

    /// <summary>
    ///     Arguments accepted by the <c>show</c> command.
    /// </summary>
    public class Settings : CommandSettings
    {
        /// <summary>
        ///     Gets the identifier of the article to display.
        /// </summary>
        [CommandArgument(0, "<ARTICLE-ID>")]
        [Description("Identifier of the article to display.")]
        public int Id { get; init; }
    }
}
