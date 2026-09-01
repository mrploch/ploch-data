using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository;
using Ploch.Data.SampleApp.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ploch.Data.SampleApp.ConsoleApp.Commands;

/// <summary>
///     Finds articles whose title contains the supplied text, demonstrating a filtered <c>GetAllAsync</c> query.
/// </summary>
/// <param name="scopeFactory">The factory used to open a dependency-injection scope for the command.</param>
public class SearchArticlesCommand(IServiceScopeFactory scopeFactory) : SampleAppCommand<SearchArticlesCommand.Settings>(scopeFactory)
{
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(IServiceProvider services, Settings settings, CancellationToken cancellationToken)
    {
        var repository = services.GetRequiredService<IReadRepositoryAsync<Article, int>>();

        var text = settings.Text;
        var articles = await repository.GetAllAsync(onDbSet: query => query.Where(article => article.Title.Contains(text)), cancellationToken: cancellationToken);

        AnsiConsole.MarkupLine($"Articles whose title contains [green]{text.EscapeMarkup()}[/]: [blue]{articles.Count}[/]");

        foreach (var article in articles)
        {
            AnsiConsole.MarkupLine($"  - [blue]{article.Id}[/] {article.Title.EscapeMarkup()}");
        }

        return 0;
    }

    /// <summary>
    ///     Arguments accepted by the <c>search</c> command.
    /// </summary>
    public class Settings : CommandSettings
    {
        /// <summary>
        ///     Gets the text that a matching article title must contain.
        /// </summary>
        [CommandArgument(0, "<TEXT>")]
        [Description("Text that a matching article title must contain.")]
        public string Text { get; init; } = string.Empty;

        /// <inheritdoc />
        public override ValidationResult Validate() =>
            string.IsNullOrWhiteSpace(Text) ? ValidationResult.Error("The search text must not be empty.") : ValidationResult.Success();
    }
}
