using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository;
using Ploch.Data.SampleApp.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ploch.Data.SampleApp.ConsoleApp.Commands;

/// <summary>
///     Renames an article through the unit of work, demonstrating automatic <c>ModifiedTime</c> tracking.
/// </summary>
/// <param name="scopeFactory">The factory used to open a dependency-injection scope for the command.</param>
public class UpdateArticleCommand(IServiceScopeFactory scopeFactory) : SampleAppCommand<UpdateArticleCommand.Settings>(scopeFactory)
{
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(IServiceProvider services, Settings settings, CancellationToken cancellationToken)
    {
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        var repository = unitOfWork.Repository<Article, int>();

        var article = await repository.GetByIdAsync(settings.Id, cancellationToken: cancellationToken);

        if (article is null)
        {
            AnsiConsole.MarkupLine($"[red]No article with Id {settings.Id} was found.[/]");

            return 1;
        }

        var originalTitle = article.Title;
        var originalModifiedTime = article.ModifiedTime;

        article.Title = settings.Title;
        await repository.UpdateAsync(article, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        AnsiConsole.MarkupLine($"Renamed [grey]{originalTitle.EscapeMarkup()}[/] to [green]{article.Title.EscapeMarkup()}[/].");
        AnsiConsole.MarkupLine($"  ModifiedTime changed from [blue]{originalModifiedTime}[/] to [blue]{article.ModifiedTime}[/].");

        return 0;
    }

    /// <summary>
    ///     Arguments and options accepted by the <c>update</c> command.
    /// </summary>
    public class Settings : CommandSettings
    {
        /// <summary>
        ///     The maximum title length the model allows, mirroring the <c>[MaxLength(256)]</c> attribute on
        ///     <see cref="Article.Title" />. SQLite silently accepts a longer value, but SQL Server — which this
        ///     sample also ships a provider project for — rejects it, so the command line is validated instead.
        /// </summary>
        public const int MaximumTitleLength = 256;

        /// <summary>
        ///     Gets the identifier of the article to rename.
        /// </summary>
        [CommandArgument(0, "<ARTICLE-ID>")]
        [Description("Identifier of the article to rename.")]
        public int Id { get; init; }

        /// <summary>
        ///     Gets the new title for the article.
        /// </summary>
        [CommandOption("-t|--title <TITLE>")]
        [Description("New title for the article.")]
        public string Title { get; init; } = string.Empty;

        /// <inheritdoc />
        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(Title))
            {
                return ValidationResult.Error("--title must be supplied and must not be empty.");
            }

            return Title.Length > MaximumTitleLength
                       ? ValidationResult.Error($"--title must be {MaximumTitleLength} characters or fewer.")
                       : ValidationResult.Success();
        }
    }
}
