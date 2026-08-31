using Microsoft.Extensions.DependencyInjection;
using Ploch.Data.GenericRepository;
using Ploch.Data.SampleApp.Model;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ploch.Data.SampleApp.ConsoleApp.Commands;

/// <summary>
///     Lists every author, demonstrating direct injection of a read-only repository.
/// </summary>
/// <param name="scopeFactory">The factory used to open a dependency-injection scope for the command.</param>
public class ListAuthorsCommand(IServiceScopeFactory scopeFactory) : SampleAppCommand<ListAuthorsCommand.Settings>(scopeFactory)
{
    /// <inheritdoc />
    protected override async Task<int> ExecuteAsync(IServiceProvider services, Settings settings, CancellationToken cancellationToken)
    {
        var repository = services.GetRequiredService<IReadRepositoryAsync<Author, int>>();
        var authors = await repository.GetAllAsync(cancellationToken: cancellationToken);

        AnsiConsole.MarkupLine($"Authors: [blue]{authors.Count}[/]");

        foreach (var author in authors)
        {
            AnsiConsole.MarkupLine($"  - [blue]{author.Id}[/] [green]{author.Name.EscapeMarkup()}[/]: {(author.Description ?? string.Empty).EscapeMarkup()}");
        }

        return 0;
    }

    /// <summary>
    ///     The <c>authors</c> command takes no arguments or options.
    /// </summary>
    public class Settings : CommandSettings;
}
