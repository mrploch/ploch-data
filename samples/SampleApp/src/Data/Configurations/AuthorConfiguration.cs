using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ploch.Data.SampleApp.Model;

namespace Ploch.Data.SampleApp.Data.Configurations;

internal class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.HasMany(a => a.Articles)
               .WithOne(a => a.Author)
               .HasForeignKey(a => a.AuthorId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
