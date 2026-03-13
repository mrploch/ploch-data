using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ploch.Data.SampleApp.Model;

namespace Ploch.Data.SampleApp.Data.Configurations;

internal class ArticleConfiguration : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        builder.HasOne(a => a.Author)
               .WithMany(a => a.Articles)
               .HasForeignKey(a => a.AuthorId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.Categories)
               .WithMany(c => c.Articles);

        builder.HasMany(a => a.Tags)
               .WithMany(t => t.Articles);

        builder.HasMany(a => a.Properties)
               .WithOne(p => p.Article)
               .HasForeignKey(p => p.ArticleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
