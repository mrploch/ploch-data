using System.Diagnostics;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Data;
using Xunit.Abstractions;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class DataContextSqLiteInMemoryTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    private readonly ITestOutputHelper _output;

    public DataContextSqLiteInMemoryTests(ITestOutputHelper output) : base(new SqLiteDbContextConfigurator(SqLiteConnectionOptions.InMemory))
    {
        _output = output;
    }

    [Fact]
    public void DataContext_add_and_query_by_id_should_create_entities_and_find_them()
    {
        var stopWatch = Stopwatch.StartNew();

        var (blog, blogPost1, blogPost2) = EntitiesBuilder.BuildBlogEntity();

        DbContext.Blogs.Add(blog);

        var (userIdea1, userIdea2) = EntitiesBuilder.BuildUserIdeaEntities();

        DbContext.UserIdeas.AddRange(userIdea1, userIdea2);

        DbContext.SaveChanges();

        var actualBlog1 = DbContext.Blogs.Find(1);
        actualBlog1.Should().BeEquivalentTo(blog);

        var actualBlogPost1 = DbContext.BlogPosts.Find(1);
        actualBlogPost1.Should().BeEquivalentTo(blogPost1);

        var actualBlogPost2 = DbContext.BlogPosts.First(bp => bp.Name == "Blog post 2");
        actualBlogPost2.Should().BeEquivalentTo(blogPost2);

        var actualUserIdea1 = DbContext.UserIdeas.First(ui => ui.Id == userIdea1.Id);
        actualUserIdea1.Should().BeEquivalentTo(userIdea1);

        var actualUserIdea2 = DbContext.UserIdeas.First(ui => ui.Id == userIdea2.Id);
        actualUserIdea2.Should().BeEquivalentTo(userIdea2);

        stopWatch.Stop();

        var elapsed = stopWatch.Elapsed;
        _output.WriteLine($"Elapsed: {elapsed}");
    }
}