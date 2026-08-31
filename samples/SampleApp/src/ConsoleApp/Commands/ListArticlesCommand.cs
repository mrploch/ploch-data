using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository;
using Ploch.Data.SampleApp.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ploch.Data.SampleApp.ConsoleApp.Commands;

/// <summary>
///     Lists articles a page at a time, demonstrating <c>GetPageAsync</c> and <c>CountAsync</c>.
/// </summary>
/// <param name="scopeFactory">The factory used to open a dependency-injection scope for the command.</param>
public class ListArticlesCommand(IServiceScopeFactory scopeFactory) : SampleAppCommand<ListArticlesCommand.Settings>(scopeFactory)
{
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(IServiceProvider services, Settings settings, CancellationToken cancellationToken)
    {
        var repository = services.GetRequiredService<IReadRepositoryAsync<Article, int>>();

        var totalCount = await repository.CountAsync(cancellationToken: cancellationToken);
        AnsiConsole.MarkupLine($"Total articles: [blue]{totalCount}[/]");

        if (totalCount == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No articles found. Run the 'seed' command first.[/]");

            return 0;
        }

        var lastPage = (totalCount + settings.PageSize - 1) / settings.PageSize;
        var firstPage = settings.AllPages ? 1 : settings.Page;
        var finalPage = settings.AllPages ? lastPage : settings.Page;

        for (var page = firstPage; page <= finalPage; page++)
        {
            var articles = await repository.GetPageAsync(page, settings.PageSize, sortBy: article => article.Id, cancellationToken: cancellationToken);

            var table = new Table().Border(TableBorder.Rounded)
                                   .Title($"Page {page} of {lastPage} ({settings.PageSize} per page)")
                                   .AddColumn("Id")
                                   .AddColumn("Title")
                                   .AddColumn("Description");

            foreach (var article in articles)
            {
                table.AddRow(article.Id.ToString(), article.Title.EscapeMarkup(), (article.Description ?? string.Empty).EscapeMarkup());
            }

            AnsiConsole.Write(table);
        }

        return 0;
    }

    /// <summary>
    ///     Options accepted by the <c>list</c> command.
    /// </summary>
    public class Settings : CommandSettings
    {
        /// <summary>
        ///     Gets the one-based page number to display.
        /// </summary>
        [CommandOption("-p|--page <NUMBER>")]
        [Description("One-based page number to display.")]
        [DefaultValue(1)]
        public int Page { get; init; } = 1;

        /// <summary>
        ///     Gets the number of articles shown per page.
        /// </summary>
        [CommandOption("-s|--page-size <SIZE>")]
        [Description("Number of articles shown per page.")]
        [DefaultValue(5)]
        public int PageSize { get; init; } = 5;

        /// <summary>
        ///     Gets a value indicating whether every page is displayed instead of a single one.
        /// </summary>
        [CommandOption("-A|--all")]
        [Description("Display every page instead of a single one.")]
        public bool AllPages { get; init; }

        /// <inheritdoc />
        public override ValidationResult Validate()
        {
            if (Page < 1)
            {
                return ValidationResult.Error("--page must be 1 or greater.");
            }

            return PageSize < 1 ? ValidationResult.Error("--page-size must be 1 or greater.") : ValidationResult.Success();
        }
    }
}
