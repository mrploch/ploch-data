using Microsoft.EntityFrameworkCore;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadRepositoryTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public async Task GetAll_should_return_entities_with_includes()
    {
        // Arrange — seed via a separate context from the root DbContext factory. RepositoryHelper
        // creates, uses, and disposes that context internally, so the seeded entities are never in
        // the read context's identity map — GetAll genuinely re-hydrates from the database and the
        // Include() assertions below exercise the real query rather than the change tracker.
        var (_, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(CreateRootDbContext);

        // Act — exercise the read repository (the code under test).
        var repository = CreateReadRepository<BlogPost, int>();
        var blogPosts = repository.GetAll(onDbSet: query => query.Include(e => e.Tags).Include(e => e.Categories).ThenInclude(c => c.Children));

        // Assert — GetAll's return value is the observable output of the feature under test.
        blogPosts.Should().HaveCount(2);
        var actualPost1 = blogPosts.Single(p => p.Id == blogPost1.Id);

        actualPost1.Should()
                   .BeEquivalentTo(blogPost1,
                                   options => options.Excluding(member => member.Path.EndsWith(".BlogPosts"))
                                                     .Excluding(member => member.Path.EndsWith(".Parent"))
                                                     .WithEntityEquivalencyOptions());

        blogPosts.Should()
                 .ContainEquivalentOf(blogPost2,
                                      options => options.Excluding(member => member.Path.EndsWith(".BlogPosts"))
                                                        .Excluding(member => member.Path.EndsWith(".Parent"))
                                                        .WithEntityEquivalencyOptions());
        foreach (var blogPost in blogPosts)
        {
            blogPost.Tags.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task GetAll_should_return_entities_without_includes()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var (_, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(CreateRootDbContext);

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPosts = repository.GetAll();
        blogPosts.Should().HaveCount(2);

        // GetAll() without an Include does not load Categories or Tags, so they are excluded
        // from the comparison.
        blogPosts.Should()
                 .ContainEquivalentOf(blogPost1,
                                      options => options.Excluding(p => p.Categories).Excluding(p => p.Tags).Excluding(p => p.CreatedTime).Excluding(p => p.ModifiedTime));
        blogPosts.Should()
                 .ContainEquivalentOf(blogPost2,
                                      options => options.Excluding(p => p.Categories).Excluding(p => p.Tags).Excluding(p => p.CreatedTime).Excluding(p => p.ModifiedTime));
    }

    [Fact]
    public async Task GetById_should_return_entity_with_includes()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var (_, _, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(CreateRootDbContext);

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPost = repository.GetById(blogPost2.Id, query => query.Include(e => e.Tags));
        blogPost.Should().BeEquivalentTo(blogPost2, options => options.WithEntityEquivalencyOptions());
        blogPost2.Tags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_with_object_key_should_return_entity_with_includes()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var (_, _, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(CreateRootDbContext);

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPost = repository.GetById([ blogPost2.Id ]);
        blogPost.Should().BeEquivalentTo(blogPost2, options => options.Excluding(p => p.Categories).Excluding(p => p.Tags).WithEntityEquivalencyOptions());
        blogPost2.Tags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPage_should_return_a_page_of_entities_with_includes()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(CreateRootDbContext, 20);

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPosts = repository.GetPage(2, 5, onDbSet: query => query.Include(e => e.Tags).Include(e => e.Categories));

        blogPosts.Should().HaveCount(5);

        for (var i = 5; i <= 9; i++)
        {
            var blogPost = posts[i];
            blogPosts.Should()
                     .ContainEquivalentOf(blogPost,
                                          options => options.Excluding(member => member.Path.EndsWith(".BlogPosts"))
                                                            .Excluding(member => member.Path.EndsWith(".Parent"))
                                                            .Excluding(member => member.Path.EndsWith(".Children"))
                                                            .WithEntityEquivalencyOptions());
        }
    }

    [Fact]
    public async Task GetPage_should_return_a_page_of_entities_with_includes_using_query()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(CreateRootDbContext, 20);

        var repository = CreateReadRepository<BlogPost, int>();

        // Filtering goes through `query`; `sortBy` makes the page deterministic (without an explicit
        // order the DB may return filtered rows in any order and the index-based assertion below
        // would be flaky); `onDbSet` is left to do only what it is for — eager loading.
        var blogPosts = repository.GetPage(2,
                                           3,
                                           sortBy: post => post.Id,
#pragma warning disable SA1117 // Parameters should be placed on the same line
                                           query: post => post.Name == "Blog post 5" || post.Name == "Blog post 6" || post.Name == "Blog post 7" ||
                                                          post.Name == "Blog post 8" || post.Name == "Blog post 9" || post.Name == "Blog post 10",
#pragma warning restore SA1117
                                           onDbSet: query => query.Include(e => e.Tags).Include(e => e.Categories));

        blogPosts.Should().HaveCount(3);

        for (var i = 7; i <= 9; i++)
        {
            var blogPost = posts[i];
            blogPosts.Should()
                     .ContainEquivalentOf(blogPost,
                                          options => options.Excluding(member => member.Path.EndsWith(".BlogPosts"))
                                                            .Excluding(member => member.Path.EndsWith(".Parent"))
                                                            .Excluding(member => member.Path.EndsWith(".Children"))
                                                            .WithEntityEquivalencyOptions());
        }
    }

    [Fact]
    public async Task GetPage_should_return_a_page_of_entities_without_includes()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(CreateRootDbContext, 20);

        var repository = CreateReadRepository<BlogPost, int>();

        // Explicit OrderBy so the page contents are deterministic and posts[i + 5] below
        // reliably match the returned slice.
        var blogPosts = repository.GetPage(2, 5, onDbSet: q => q.OrderBy(e => e.Id));

        blogPosts.Should().HaveCount(5);

        for (var i = 0; i < 5; i++)
        {
            var blogPost = posts[i + 5];
            var queriedPost = blogPosts.Should().ContainEquivalentOf(blogPost, options => options.Excluding(p => p.Categories).Excluding(p => p.Tags)).Subject;
            queriedPost.Tags.Should().BeEmpty();
            queriedPost.Categories.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Count_should_return_entity_count()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(CreateRootDbContext, 20);

        var repository = CreateReadRepository<BlogPost, int>();
        var count = repository.Count();

        count.Should().Be(20);
    }

    [Fact]
    public async Task Find_should_query_repository_for_first_entity_and_return_it()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var testBlogEntities = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(CreateRootDbContext);

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPost = repository.FindFirst(post => post.Name.Contains("Blog post 1"));

        blogPost.Should().NotBeNull();
        blogPost.Should().BeEquivalentTo(testBlogEntities.blogPost1, options => options.Excluding(p => p.Categories).Excluding(p => p.Tags).WithEntityEquivalencyOptions());
    }

    [Fact]
    public async Task Find_with_OnDbSet_action_should_apply_the_shaping_to_the_query()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var tags = await RepositoryHelper.AddBlogPostTagsAsync(CreateRootDbContext, 10);

        var repository = CreateReadRepository<BlogPostTag, int>();

        // The predicate matches three rows, so the ordering applied through onDbSet decides which
        // one FindFirst returns — that is what makes the shaping observable. Filtering stays in
        // `query`; onDbSet is never used to narrow the result set.
        var blogPostTag = repository.FindFirst(postTag => postTag.Name == tags[3].Name || postTag.Name == tags[4].Name || postTag.Name == tags[5].Name,
                                               blogPostTags => blogPostTags.OrderByDescending(tag => tag.Id));

        blogPostTag.Should().NotBeNull();
        blogPostTag.Should().BeEquivalentTo(tags[5], options => options.WithEntityEquivalencyOptions());
    }

    [Fact]
    public async Task GetAll_should_filter_the_entities_using_the_query()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(CreateRootDbContext, 20);

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPosts = repository.GetAll(post => post.Id > posts[14].Id);

        blogPosts.Should().HaveCount(5);
        blogPosts.Should().OnlyContain(post => post.Id > posts[14].Id);
    }

    [Fact]
    public async Task GetAll_should_apply_the_query_and_the_shaping_together()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var (_, blogPost1, _) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(CreateRootDbContext);

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPosts = repository.GetAll(post => post.Id == blogPost1.Id, query => query.Include(post => post.Tags));

        // `query` narrowed the result set to the right entity — assert the identity explicitly, not
        // just the count, so a mis-composed predicate returning the *other* post is caught here
        // rather than surviving on an incidental difference in tag counts.
        blogPosts.Should().ContainSingle();
        blogPosts[0].Id.Should().Be(blogPost1.Id);

        // …and `onDbSet` eager-loaded the navigation collection.
        blogPosts[0].Tags.Should().NotBeEmpty().And.HaveCount(blogPost1.Tags.Count);
    }

    [Fact]
    public async Task Count_should_count_only_the_entities_matching_the_query()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(CreateRootDbContext, 20);

        var repository = CreateReadRepository<BlogPost, int>();

        repository.Count(post => post.Id > posts[14].Id).Should().Be(5);
        repository.Count(post => post.Id < posts[0].Id).Should().Be(0);
    }

    [Fact]
    public async Task GetPage_should_order_the_results_using_sortBy()
    {
        // Arrange — seed via a separate context (see GetAll_should_return_entities_with_includes
        // for why the read must not share the seed context's change tracker).
        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(CreateRootDbContext, 20);

        // Sort descending on purpose. Ascending by Id coincides with the natural insertion/rowid
        // order, so an ascending assertion would still pass if sortBy were ignored entirely.
        var expectedSecondPageIds = posts.Select(post => post.Id).OrderDescending().Skip(5).Take(5).ToList();

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPosts = repository.GetPage(2, 5, sortBy: post => -post.Id);

        blogPosts.Select(post => post.Id).Should().Equal(expectedSecondPageIds);
    }

    [Fact]
    public void Count_should_return_zero_when_repository_is_empty()
    {
        var repository = CreateReadRepository<BlogPost, int>();
        var count = repository.Count();

        count.Should().Be(0);
    }
}
