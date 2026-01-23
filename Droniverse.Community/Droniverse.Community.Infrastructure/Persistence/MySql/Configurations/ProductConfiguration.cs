using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product");

        builder.HasKey(c => c.ProductID);
        builder.Property(c => c.ProductID).HasColumnType("char(36)");
        builder.Property(c => c.CategoryID).HasColumnType("char(36)");
        builder.Property(c => c.ReferenceID).HasColumnType("char(36)");
        builder.HasOne(p => p.ProductCategory)
            .WithMany(pc => pc.Products)
            .HasForeignKey(p => p.CategoryID)
            .OnDelete(DeleteBehavior.Restrict);
        //builder.HasOne(p => p.Code) // ReferenceID của Course
        //    .WithMany(c => c.Products)
        //    .HasForeignKey(p => p.ReferenceID)
        //    .OnDelete(DeleteBehavior.Restrict);
        //ReferenceID của Drone => bên service Simulation => K cần qhe ở đây
        
        builder.Property(c => c.ProductNameVN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.ProductNameEN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.DescriptionEN).HasColumnType("text");
        builder.Property(c => c.DescriptionVN).HasColumnType("text");
        builder.Property(p => p.Price).HasColumnType("decimal(14, 9)");
        builder.Property(p => p.Status).HasColumnType("tinyint").HasConversion<byte>();
        builder.Property(p => p.Currency).HasMaxLength(10).HasConversion<string>().HasDefaultValue(CurrencyType.VND);
        
        builder.Property(p => p.CreateAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(p => p.UpdateAt).HasColumnType("datetime").ValueGeneratedOnUpdate();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Product_Status", "`Status` IN (1, 2)");
            t.HasCheckConstraint("CK_Product_Currency", "`Currency` IN ('VND', 'USD')");
        });
    }
}

