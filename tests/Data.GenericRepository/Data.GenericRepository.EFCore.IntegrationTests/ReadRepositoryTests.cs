using Microsoft.EntityFrameworkCore;
using Ploch.Data.GenericRepository.EFCore.IntegrationTesting;
using Ploch.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Data.GenericRepository.EFCore.IntegrationTests;

public class ReadRepositoryTests : GenericRepositoryDataIntegrationTest<TestDbContext>
{
    [Fact]
    public async Task GetAll_should_return_entities_with_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (_, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPosts = repository.GetAll(query => query.Include(e => e.Tags).Include(e => e.Categories).ThenInclude(c => c.Children));
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
        using var unitOfWork = CreateUnitOfWork();

        var (_, blogPost1, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPosts = repository.GetAll();
        blogPosts.Should().HaveCount(2);
        blogPosts.Should()
                 .ContainEquivalentOf(blogPost1,
                                      options => options.Excluding(p => p.Categories).Excluding(p => p.Tags).Excluding(p => p.CreatedTime).Excluding(p => p.ModifiedTime));
        blogPosts.Should()
                 .ContainEquivalentOf(blogPost2,
                                      options => options.Excluding(p => p.Categories).Excluding(p => p.Tags).Excluding(p => p.CreatedTime).Excluding(p => p.ModifiedTime));

        // Categories and Tags are not included in the query but they will not be empty because they are already loaded in the context.
        // This is why I don't check for emptiness here.
    }

    [Fact]
    public async Task GetById_should_return_entity_with_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (_, _, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPost = repository.GetById(blogPost2.Id, query => query.Include(e => e.Tags));
        blogPost.Should().BeEquivalentTo(blogPost2, options => options.WithEntityEquivalencyOptions());
        blogPost2.Tags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_with_object_key_should_return_entity_with_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (_, _, blogPost2) = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPost = repository.GetById([ blogPost2.Id ]);
        blogPost.Should().BeEquivalentTo(blogPost2, options => options.Excluding(p => p.Categories).Excluding(p => p.Tags).WithEntityEquivalencyOptions());
        blogPost2.Tags.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPage_should_return_a_page_of_entities_with_includes()
    {
        using var unitOfWork = CreateUnitOfWork();

        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(unitOfWork.Repository<Blog, int>(), 20);

        await unitOfWork.CommitAsync();

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
        using var unitOfWork = CreateUnitOfWork();

        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(unitOfWork.Repository<Blog, int>(), 20);

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPosts = repository.GetPage(2,
                                           3,
#pragma warning disable SA1117 // Parameters should be placed on the same line
                                           query => query.Name == "Blog post 5" || query.Name == "Blog post 6" || query.Name == "Blog post 7" || query.Name == "Blog post 8" ||
                                                    query.Name == "Blog post 9" || query.Name == "Blog post 10",
#pragma warning restore SA1117
                                           // Explicit OrderBy so page contents are deterministic — without it, the
                                           // DB may return filtered rows in any order and the index-based assertion below would be flaky.
                                           query => query.OrderBy(e => e.Id).Include(e => e.Tags).Include(e => e.Categories));

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
        using var unitOfWork = CreateUnitOfWork();

        var (_, posts) = await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(unitOfWork.Repository<Blog, int>(), 20);

        await unitOfWork.CommitAsync();

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
        using var unitOfWork = CreateUnitOfWork();

        await RepositoryHelper.AddAsyncTestBlogEntitiesWithManyPostsAsync(unitOfWork.Repository<Blog, int>(), 20);

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<BlogPost, int>();
        var count = repository.Count();

        count.Should().Be(20);
    }

    [Fact]
    public async Task Find_should_query_repository_for_first_entity_and_return_it()
    {
        using var unitOfWork = CreateUnitOfWork();

        var testBlogEntities = await RepositoryHelper.AddAsyncTestBlogEntitiesAsync(unitOfWork.Repository<Blog, int>());

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<BlogPost, int>();
        var blogPost = repository.FindFirst(post => post.Name.Contains("Blog post 1"));

        blogPost.Should().NotBeNull();
        blogPost.Should().BeEquivalentTo(testBlogEntities.blogPost1, options => options.Excluding(p => p.Categories).Excluding(p => p.Tags).WithEntityEquivalencyOptions());
    }

    [Fact]
    public async Task Find_with_OnDbSet_action_should_query_repository_for_first_entity_and_return_it()
    {
        using var unitOfWork = CreateUnitOfWork();

        var tags = await RepositoryHelper.AddBlogPostTagsAsync(unitOfWork.Repository<BlogPostTag, int>(), 10);

        await unitOfWork.CommitAsync();

        var repository = CreateReadRepository<BlogPostTag, int>();

        var blogPostTag = repository.FindFirst(postTag => postTag.Name.Contains(tags[3].Name) || postTag.Name.Contains(tags[4].Name) || postTag.Name.Contains(tags[5].Name),
                                               blogPostTags => blogPostTags.Where(tag => tag.Name.Contains(tags[4].Name)));

        blogPostTag.Should().NotBeNull();
        blogPostTag.Should().BeEquivalentTo(tags[4]);
    }

    [Fact]
    public void Count_should_return_zero_when_repository_is_empty()
    {
        using var unitOfWork = CreateUnitOfWork();

        var repository = CreateReadRepository<BlogPost, int>();
        var count = repository.Count();

        count.Should().Be(0);
    }
}
