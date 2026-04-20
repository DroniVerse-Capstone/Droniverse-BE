using Droniverse.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Identity.Infrastructure.Persistence.Configurations;

public class SysPolicyConfiguration : IEntityTypeConfiguration<SysPolicy>
{
    public void Configure(EntityTypeBuilder<SysPolicy> builder)
    {
        builder.ToTable("SysPolicy");
        builder.HasKey(sp => sp.SysPolicyID);

        builder.HasOne(sp => sp.CreatedByUser)
            .WithMany()
            .HasForeignKey(sp => sp.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sp => sp.UpdatedByUser)
            .WithMany()
            .HasForeignKey(sp => sp.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(sp => sp.Type)
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion<string>();
        builder.Property(sp => sp.Title)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(sp => sp.Content)
            .HasColumnType("text")
            .IsRequired();
        builder.Property(sp => sp.EffectiveDate)
            .HasColumnType("datetime")
            .IsRequired();
        builder.Property(sp => sp.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();
        builder.Property(sp => sp.CreatedBy)
            .HasColumnType("char(36)")
            .IsRequired();
        builder.Property(sp => sp.UpdatedAt)
            .HasColumnType("datetime")
            .IsRequired();
        builder.Property(sp => sp.UpdatedBy)
            .HasColumnType("char(36)")
            .IsRequired();


    }
}

