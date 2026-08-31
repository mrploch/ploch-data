using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Ploch.Data.SampleApp.ConsoleApp.Commands;

/// <summary>
///     Base class for every command in the sample application.
/// </summary>
/// <remarks>
///     Commands themselves are resolved from the root container by Spectre.Console.Cli, whereas the
///     <c>DbContext</c> and the repositories registered by <c>AddDbContextWithRepositories</c> are scoped.
///     This base class therefore opens an explicit dependency-injection scope for the duration of the command,
///     which is the same pattern a hosted service or a request handler would use.
/// </remarks>
/// <typeparam name="TSettings">The settings type describing the command's arguments and options.</typeparam>
/// <param name="scopeFactory">The factory used to open a dependency-injection scope for the command.</param>
public abstract class SampleAppCommand<TSettings>(IServiceScopeFactory scopeFactory) : AsyncCommand<TSettings>
    where TSettings : CommandSettings
{
    /// <inheritdoc />
    public override Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken) =>
        ExecuteAsync(settings, cancellationToken);

    /// <summary>
    ///     Runs the command without going through Spectre.Console.Cli's argument parsing.
    /// </summary>
    /// <remarks>
    ///     This is the seam the end-to-end tests use: it exercises exactly the code the parsed command line
    ///     would reach, without needing a <see cref="CommandContext" />.
    /// </remarks>
    /// <param name="settings">The settings the command runs with.</param>
    /// <param name="cancellationToken">A token that signals the command should stop work.</param>
    /// <returns>The process exit code; <c>0</c> indicates success.</returns>
    public async Task<int> ExecuteAsync(TSettings settings, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();

        return await ExecuteAsync(scope.ServiceProvider, settings, cancellationToken);
    }

    /// <summary>
    ///     Runs the command against a freshly opened dependency-injection scope.
    /// </summary>
    /// <param name="services">The scoped service provider from which repositories and the unit of work are resolved.</param>
    /// <param name="settings">The parsed command settings.</param>
    /// <param name="cancellationToken">A token that signals the command should stop work.</param>
    /// <returns>The process exit code; <c>0</c> indicates success.</returns>
    protected abstract Task<int> ExecuteAsync(IServiceProvider services, TSettings settings, CancellationToken cancellationToken);
}
