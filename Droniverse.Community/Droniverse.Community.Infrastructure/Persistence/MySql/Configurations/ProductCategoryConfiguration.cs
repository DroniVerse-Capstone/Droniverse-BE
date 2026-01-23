using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategory");

        builder.HasKey(c => c.CategoryID);
        builder.Property(c => c.CategoryID).HasColumnType("char(36)");
        builder.HasMany(c => c.Products).WithOne(p => p.ProductCategory);

        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.CategoryNameVN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.CategoryNameEN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.DescriptionEN).HasColumnType("text");
        builder.Property(c => c.DescriptionVN).HasColumnType("text");
    }
}

