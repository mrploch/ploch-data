using Microsoft.EntityFrameworkCore;
using Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Model;

namespace Ploch.Common.Data.GenericRepository.EFCore.IntegrationTests.Data;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    { }

    public DbSet<Blog> Blogs { get; set; } = null!;

    public DbSet<BlogPost> BlogPosts { get; set; } = null!;

    public DbSet<BlogPostCategory> BlogPostCategories { get; set; } = null!;

    public DbSet<BlogPostTag> BlogPostTags { get; set; } = null!;

    public DbSet<UserIdea> UserIdeas { get; set; } = null!;

    public DbSet<TestEntity> TestEntities { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Blog>().HasMany(static b => b.BlogPosts).WithOne().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BlogPost>().HasMany(static bp => bp.Categories).WithMany(c => c.BlogPosts);
        modelBuilder.Entity<BlogPost>().HasMany(static bp => bp.Tags).WithMany(t => t.BlogPosts);
        modelBuilder.Entity<BlogPostCategory>().HasMany(static c => c.Children).WithOne(c => c.Parent).OnDelete(DeleteBehavior.ClientCascade);
        modelBuilder.Entity<BlogPostTag>().HasKey(static bpt => bpt.Id);
        modelBuilder.Entity<UserIdea>().HasKey(static ui => ui.Id);
    }
}