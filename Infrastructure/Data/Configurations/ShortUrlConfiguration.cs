using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class ShortUrlConfiguration : IEntityTypeConfiguration<ShortUrl>
    {
        public void Configure(EntityTypeBuilder<ShortUrl> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OriginalUrl)
                .IsRequired();

            builder.Property(x => x.ShortCode)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.CreatedDate)
                .IsRequired();

            builder.HasIndex(x => x.OriginalUrl)
                .IsUnique();

            builder.HasIndex(x => x.ShortCode)
                .IsUnique();
        }
    }
}