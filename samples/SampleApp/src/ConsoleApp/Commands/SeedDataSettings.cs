using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ploch.Data.SampleApp.ConsoleApp.Commands;

/// <summary>
///     Options shared by every command that creates the sample data set.
/// </summary>
/// <remarks>
///     Both <c>seed</c> and <c>demo</c> populate the database through
///     <see cref="Services.SampleDataSeeder" />, so they accept the same author and filler-article options.
///     Declaring them once here keeps the two commands' command lines identical by construction.
/// </remarks>
public class SeedDataSettings : CommandSettings
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
