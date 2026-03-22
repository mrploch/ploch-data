using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ploch.Data.GenericRepository;
using Ploch.Data.SampleApp.Data;
using Ploch.Data.SampleApp.Model;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSampleAppDataServices(
    options => options.UseSqlite("Data Source=sampleapp.db"),
    builder.Configuration);

using var host = builder.Build();

using var scope = host.Services.CreateScope();
var services = scope.ServiceProvider;

var dbContext = services.GetRequiredService<SampleAppDbContext>();
await dbContext.Database.EnsureDeletedAsync();
await dbContext.Database.EnsureCreatedAsync();

var unitOfWork = services.GetRequiredService<IUnitOfWork>();

Console.WriteLine("=== Ploch.Data Sample Application ===");
Console.WriteLine();

// 1. Create an Author
Console.WriteLine("--- Creating an Author ---");
var authorRepo = unitOfWork.Repository<Author, int>();
var author = new Author { Name = "Jane Smith", Description = "Technical writer and software engineer" };
await authorRepo.AddAsync(author);
await unitOfWork.CommitAsync();
Console.WriteLine($"Created author: {author.Name} (Id: {author.Id})");
Console.WriteLine($"  CreatedTime automatically set: {author.CreatedTime}");
Console.WriteLine();

// 2. Create Hierarchical Categories
Console.WriteLine("--- Creating Hierarchical Categories ---");
var categoryRepo = unitOfWork.Repository<ArticleCategory, int>();
var techCategory = new ArticleCategory { Name = "Technology" };
var dotnetCategory = new ArticleCategory { Name = ".NET", Parent = techCategory };
var efCoreCategory = new ArticleCategory { Name = "Entity Framework Core", Parent = dotnetCategory };
var scienceCategory = new ArticleCategory { Name = "Science" };

await categoryRepo.AddAsync(techCategory);
await categoryRepo.AddAsync(scienceCategory);
await unitOfWork.CommitAsync();
Console.WriteLine("Created category hierarchy: Technology > .NET > Entity Framework Core");
Console.WriteLine("Created standalone category: Science");
Console.WriteLine();

// 3. Create Tags
Console.WriteLine("--- Creating Tags ---");
var tagRepo = unitOfWork.Repository<ArticleTag, int>();
var csharpTag = new ArticleTag { Name = "C#", Description = "C# programming language" };
var tutorialTag = new ArticleTag { Name = "Tutorial", Description = "Step-by-step guide" };
var beginnerTag = new ArticleTag { Name = "Beginner", Description = "Suitable for beginners" };
await tagRepo.AddAsync(csharpTag);
await tagRepo.AddAsync(tutorialTag);
await tagRepo.AddAsync(beginnerTag);
await unitOfWork.CommitAsync();
Console.WriteLine($"Created {await tagRepo.CountAsync()} tags");
Console.WriteLine();

// 4. Create Articles with Categories, Tags, and Properties
Console.WriteLine("--- Creating Articles ---");
var articleRepo = unitOfWork.Repository<Article, int>();

var article1 = new Article
{
    Title = "Getting Started with Entity Framework Core",
    Description = "A beginner's guide to EF Core",
    Contents = "Entity Framework Core is a modern ORM for .NET...",
    Author = author,
    Categories = new List<ArticleCategory> { dotnetCategory, efCoreCategory },
    Tags = new List<ArticleTag> { csharpTag, tutorialTag, beginnerTag },
    Properties = new List<ArticleProperty>
    {
        new() { Name = "ReadingTime", Value = "10 minutes" },
        new() { Name = "Difficulty", Value = "Beginner" }
    }
};

var article2 = new Article
{
    Title = "Advanced Repository Patterns",
    Description = "Deep dive into the Generic Repository pattern",
    Contents = "The Generic Repository pattern provides a clean abstraction...",
    Author = author,
    Categories = new List<ArticleCategory> { dotnetCategory },
    Tags = new List<ArticleTag> { csharpTag }
};

await articleRepo.AddAsync(article1);
await articleRepo.AddAsync(article2);
await unitOfWork.CommitAsync();
Console.WriteLine($"Created article: '{article1.Title}' with {article1.Categories?.Count} categories, {article1.Tags.Count} tags, and {article1.Properties.Count} properties");
Console.WriteLine($"Created article: '{article2.Title}'");
Console.WriteLine($"  Audit - CreatedTime: {article1.CreatedTime}, ModifiedTime: {article1.ModifiedTime}");
Console.WriteLine();

// 5. Read Articles with eager loading
Console.WriteLine("--- Reading Articles with Includes ---");
var readArticleRepo = services.GetRequiredService<IReadRepositoryAsync<Article, int>>();
var loadedArticle = await readArticleRepo.GetByIdAsync(
    article1.Id,
    onDbSet: q => q.Include(a => a.Author)
                   .Include(a => a.Categories)
                   .Include(a => a.Tags)
                   .Include(a => a.Properties));

Console.WriteLine($"Loaded article: '{loadedArticle!.Title}'");
Console.WriteLine($"  Author: {loadedArticle.Author?.Name}");
Console.WriteLine($"  Categories: {string.Join(", ", loadedArticle.Categories?.Select(c => c.Name) ?? [])}");
Console.WriteLine($"  Tags: {string.Join(", ", loadedArticle.Tags.Select(t => t.Name))}");
Console.WriteLine($"  Properties: {string.Join(", ", loadedArticle.Properties.Select(p => $"{p.Name}={p.Value}"))}");
Console.WriteLine();

// 6. Update an Article (demonstrates ModifiedTime tracking)
Console.WriteLine("--- Updating an Article ---");
var originalModifiedTime = article1.ModifiedTime;
await Task.Delay(100); // Small delay to ensure different timestamp
article1.Title = "Getting Started with Entity Framework Core (Updated)";
await articleRepo.UpdateAsync(article1);
await unitOfWork.CommitAsync();
Console.WriteLine($"Updated article title to: '{article1.Title}'");
Console.WriteLine($"  ModifiedTime changed from {originalModifiedTime} to {article1.ModifiedTime}");
Console.WriteLine();

// 7. Demonstrate Pagination
Console.WriteLine("--- Pagination ---");

// Add more articles for pagination demo
for (var i = 3; i <= 12; i++)
{
    await articleRepo.AddAsync(new Article
    {
        Title = $"Sample Article {i}",
        Description = $"Description for article {i}",
        Contents = $"Contents of article {i}",
        Author = author
    });
}

await unitOfWork.CommitAsync();

var totalCount = await articleRepo.CountAsync();
Console.WriteLine($"Total articles: {totalCount}");

var page1 = await readArticleRepo.GetPageAsync(1, 5);
Console.WriteLine($"Page 1 (5 per page): {string.Join(", ", page1.Select(a => a.Title))}");

var page2 = await readArticleRepo.GetPageAsync(2, 5);
Console.WriteLine($"Page 2 (5 per page): {string.Join(", ", page2.Select(a => a.Title))}");
Console.WriteLine();

// 8. Demonstrate GetAllAsync with filter
Console.WriteLine("--- Filtered Queries ---");
var allArticles = await readArticleRepo.GetAllAsync(
    onDbSet: q => q.Where(a => a.Title.Contains("Entity Framework")));
Console.WriteLine($"Articles about Entity Framework: {allArticles.Count}");
foreach (var article in allArticles)
{
    Console.WriteLine($"  - {article.Title}");
}

Console.WriteLine();

// 9. Demonstrate direct repository injection (read-only)
Console.WriteLine("--- Direct Repository Injection ---");
var readOnlyAuthorRepo = services.GetRequiredService<IReadRepositoryAsync<Author, int>>();
var allAuthors = await readOnlyAuthorRepo.GetAllAsync();
Console.WriteLine($"Authors ({allAuthors.Count}):");
foreach (var a in allAuthors)
{
    Console.WriteLine($"  - {a.Name}: {a.Description}");
}

Console.WriteLine();
Console.WriteLine("=== Sample Application Complete ===");
