using Ploch.Data.SampleApp.Model;

namespace Ploch.Data.SampleApp.ConsoleApp.Services;

/// <summary>
///     Describes the data created by <see cref="SampleDataSeeder.SeedAsync" />.
/// </summary>
/// <param name="Author">The author the seeded articles are attributed to.</param>
/// <param name="FeaturedArticle">The fully populated article carrying categories, tags, and properties.</param>
/// <param name="SecondArticle">The second hand-written article.</param>
/// <param name="CategoryCount">The total number of categories in the database after seeding.</param>
/// <param name="TagCount">The total number of tags in the database after seeding.</param>
/// <param name="ArticleCount">The total number of articles in the database after seeding.</param>
public record SeedResult(Author Author, Article FeaturedArticle, Article SecondArticle, int CategoryCount, int TagCount, int ArticleCount);
