using Microsoft.EntityFrameworkCore;
using Ploch.Data.EFCore.SqLite;
using Ploch.Data.Model;
using Ploch.Data.SampleApp.Model;

namespace Ploch.Data.SampleApp.Data;

public class SampleAppDbContext : DbContext
{
    public SampleAppDbContext(DbContextOptions<SampleAppDbContext> options) : base(options)
    { }

    protected SampleAppDbContext()
    { }

    public DbSet<Article> Articles { get; set; } = null!;

    public DbSet<ArticleCategory> ArticleCategories { get; set; } = null!;

    public DbSet<ArticleTag> ArticleTags { get; set; } = null!;

    public DbSet<ArticleProperty> ArticleProperties { get; set; } = null!;

    public DbSet<Author> Authors { get; set; } = null!;

    public override int SaveChanges()
    {
        SetAuditTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SampleAppDbContext).Assembly);
        modelBuilder.ApplySqLiteDateTimeOffsetPropertiesFix(Database);
        base.OnModelCreating(modelBuilder);
    }

    private void SetAuditTimestamps()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IHasAuditTimeProperties>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedTime = now;
                    entry.Entity.ModifiedTime = now;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedTime = now;
                    break;
            }
        }
    }
}
