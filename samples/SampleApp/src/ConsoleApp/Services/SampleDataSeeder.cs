using Microsoft.EntityFrameworkCore;
using Ploch.Data.GenericRepository;
using Ploch.Data.SampleApp.Data;
using Ploch.Data.SampleApp.Model;

namespace Ploch.Data.SampleApp.ConsoleApp.Services;

/// <summary>
///     Creates the sample data set used by every command in this application.
/// </summary>
/// <remarks>
///     The seeder demonstrates writing through <see cref="IUnitOfWork" />: repositories are obtained from the
///     unit of work, entities of several types are added, and a single <c>CommitAsync</c> persists them atomically.
/// </remarks>
/// <param name="unitOfWork">The unit of work used to obtain repositories and commit the changes.</param>
/// <param name="dbContext">The database context, used to create the database schema.</param>
public class SampleDataSeeder(IUnitOfWork unitOfWork, SampleAppDbContext dbContext)
{
    /// <summary>
    ///     Drops the database if it exists and recreates it from the current model.
    /// </summary>
    /// <param name="cancellationToken">A token that signals the operation should stop.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ResetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
    }

    /// <summary>
    ///     Ensures the database exists without discarding any data already in it.
    /// </summary>
    /// <param name="cancellationToken">A token that signals the operation should stop.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task EnsureDatabaseAsync(CancellationToken cancellationToken = default) => dbContext.Database.EnsureCreatedAsync(cancellationToken);

    /// <summary>
    ///     Seeds an author, a hierarchical category tree, tags, and articles with properties.
    /// </summary>
    /// <param name="authorName">The name of the author the seeded articles are attributed to.</param>
    /// <param name="fillerArticleCount">The number of additional filler articles created to make pagination meaningful.</param>
    /// <param name="cancellationToken">A token that signals the operation should stop.</param>
    /// <returns>A summary of what was created.</returns>
    public async Task<SeedResult> SeedAsync(string authorName, int fillerArticleCount, CancellationToken cancellationToken = default)
    {
        var authorRepository = unitOfWork.Repository<Author, int>();
        var author = new Author { Name = authorName, Description = "Technical writer and software engineer" };
        await authorRepository.AddAsync(author, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var categoryRepository = unitOfWork.Repository<ArticleCategory, int>();
        var technologyCategory = new ArticleCategory { Name = "Technology" };
        var dotnetCategory = new ArticleCategory { Name = ".NET", Parent = technologyCategory };
        var efCoreCategory = new ArticleCategory { Name = "Entity Framework Core", Parent = dotnetCategory };
        var scienceCategory = new ArticleCategory { Name = "Science" };

        await categoryRepository.AddAsync(technologyCategory, cancellationToken);
        await categoryRepository.AddAsync(scienceCategory, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var tagRepository = unitOfWork.Repository<ArticleTag, int>();
        var csharpTag = new ArticleTag { Name = "C#", Description = "C# programming language" };
        var tutorialTag = new ArticleTag { Name = "Tutorial", Description = "Step-by-step guide" };
        var beginnerTag = new ArticleTag { Name = "Beginner", Description = "Suitable for beginners" };
        await tagRepository.AddAsync(csharpTag, cancellationToken);
        await tagRepository.AddAsync(tutorialTag, cancellationToken);
        await tagRepository.AddAsync(beginnerTag, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var articleRepository = unitOfWork.Repository<Article, int>();

        var featuredArticle = new Article
                              {
                                  Title = "Getting Started with Entity Framework Core",
                                  Description = "A beginner's guide to EF Core",
                                  Contents = "Entity Framework Core is a modern ORM for .NET...",
                                  Author = author,
                                  Categories = [dotnetCategory, efCoreCategory],
                                  Tags = [csharpTag, tutorialTag, beginnerTag],
                                  Properties =
                                  [
                                      new ArticleProperty { Name = "ReadingTime", Value = "10 minutes" },
                                      new ArticleProperty { Name = "Difficulty", Value = "Beginner" }
                                  ]
                              };

        var secondArticle = new Article
                            {
                                Title = "Advanced Repository Patterns",
                                Description = "Deep dive into the Generic Repository pattern",
                                Contents = "The Generic Repository pattern provides a clean abstraction...",
                                Author = author,
                                Categories = [dotnetCategory],
                                Tags = [csharpTag]
                            };

        await articleRepository.AddAsync(featuredArticle, cancellationToken);
        await articleRepository.AddAsync(secondArticle, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        for (var index = 1; index <= fillerArticleCount; index++)
        {
            await articleRepository.AddAsync(new Article
                                             {
                                                 Title = $"Sample Article {index + 2}",
                                                 Description = $"Description for article {index + 2}",
                                                 Contents = $"Contents of article {index + 2}",
                                                 Author = author
                                             },
                                             cancellationToken);
        }

        if (fillerArticleCount > 0)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return new SeedResult(author,
                              featuredArticle,
                              secondArticle,
                              await categoryRepository.CountAsync(cancellationToken: cancellationToken),
                              await tagRepository.CountAsync(cancellationToken: cancellationToken),
                              await articleRepository.CountAsync(cancellationToken: cancellationToken));
    }
}
