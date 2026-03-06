using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Category");

        builder.HasKey(c => c.CategoryID);
        builder.Property(c => c.CategoryID).HasColumnType("char(36)");
        
        builder.Property(c => c.TypeNameEN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.TypeNameVN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.DescriptionEN).HasColumnType("text");
        builder.Property(c => c.DescriptionVN).HasColumnType("text");

        builder.HasMany(x => x.ClubCreationRequests)
                  .WithOne(x => x.Category)
                  .HasForeignKey(x => x.CategoryID)
                  .OnDelete(DeleteBehavior.Restrict);
    }
}

