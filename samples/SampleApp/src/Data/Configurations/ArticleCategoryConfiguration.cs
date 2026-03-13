using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ploch.Data.SampleApp.Model;

namespace Ploch.Data.SampleApp.Data.Configurations;

internal class ArticleCategoryConfiguration : IEntityTypeConfiguration<ArticleCategory>
{
    public void Configure(EntityTypeBuilder<ArticleCategory> builder)
    {
        builder.HasOne(c => c.Parent)
               .WithMany(c => c.Children)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.ClientCascade);
    }
}
