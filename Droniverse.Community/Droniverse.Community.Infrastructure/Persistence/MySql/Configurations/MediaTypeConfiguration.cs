using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class MediaTypeConfiguration : IEntityTypeConfiguration<MediaType>
{
    public void Configure(EntityTypeBuilder<MediaType> builder)
    {
        builder.ToTable("MediaType");

        builder.HasKey(c => c.MediaTypeID);
        builder.Property(c => c.MediaTypeID).HasColumnType("char(36)");
        builder.HasMany(c => c.Medias)
            .WithOne(cvc => cvc.MediaType);
        
        builder.Property(c => c.TypeNameEN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.TypeNameVN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.DescriptionEN).HasColumnType("text");
        builder.Property(c => c.DescriptionVN).HasColumnType("text");
        builder.Property(m => m.CreatedAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(m => m.UpdatedAt).HasColumnType("datetime").ValueGeneratedOnUpdate();
    }
}

