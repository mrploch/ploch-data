using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ploch.Data.SampleApp.Model;

namespace Ploch.Data.SampleApp.Data.Configurations;

/// <summary>
/// Author entity configuration.
/// The Author-Article relationship is configured in <see cref="ArticleConfiguration"/>.
/// </summary>
internal class AuthorConfiguration : Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        // Author-Article relationship is configured from the Article side in ArticleConfiguration.
        // No additional configuration needed here beyond what Data Annotations provide.
    }
}
