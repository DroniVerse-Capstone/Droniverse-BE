using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class ModuleConfiguration : IEntityTypeConfiguration<Media>
{
    public void Configure(EntityTypeBuilder<Media> builder)
    {
        builder.ToTable("Media");

        builder.HasKey(c => c.MediaID);
        builder.Property(c => c.MediaID).HasColumnType("char(36)");
        builder.Property(c => c.MediaTypeID).HasColumnType("char(36)");
        builder.HasOne(c => c.MediaType)
            .WithMany(cvc => cvc.Medias)
            .HasForeignKey(m => m.MediaTypeID)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Property(c => c.ImageUrl).HasColumnType("text");
        builder.Property(m => m.CreateAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(m => m.UpdateAt).HasColumnType("datetime").ValueGeneratedOnUpdate();
    }
}

