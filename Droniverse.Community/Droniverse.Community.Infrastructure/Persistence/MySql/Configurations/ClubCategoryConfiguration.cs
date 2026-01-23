using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class ClubCategoryConfiguration : IEntityTypeConfiguration<ClubCategory>
{
    public void Configure(EntityTypeBuilder<ClubCategory> builder)
    {
        builder.ToTable("ClubCategory");

        builder.HasKey(c => new { c.CategoryID, c.ClubID });
        builder.Property(c => c.CategoryID).HasColumnType("char(36)");
        builder.Property(c => c.ClubID).HasColumnType("char(36)");
        builder.HasOne(c => c.Category)
            .WithMany(cg => cg.ClubCategories)
            .HasForeignKey(c => c.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Club)
            .WithMany(cg => cg.ClubCategories)
            .HasForeignKey(c => c.ClubID)
            .OnDelete(DeleteBehavior.Restrict);

    }
}

