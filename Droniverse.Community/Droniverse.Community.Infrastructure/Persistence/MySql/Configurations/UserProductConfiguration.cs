using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class UserProductConfiguration : IEntityTypeConfiguration<UserProduct>
{
    public void Configure(EntityTypeBuilder<UserProduct> builder)
    {
        builder.ToTable("UserProduct");

        builder.HasKey(c => c.UserProductID);
        builder.Property(c => c.UserProductID).HasColumnType("char(36)");
        builder.Property(c => c.UserID).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.ProductID).HasColumnType("char(36)").IsRequired();
        builder.HasOne(p => p.Product)
            .WithMany(pc => pc.UserProducts)
            .HasForeignKey(p => p.ProductID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(c => c.Source).HasColumnType("text");
        builder.Property(p => p.AcquiredAt).HasColumnType("datetime").ValueGeneratedOnAdd();
    }
}

