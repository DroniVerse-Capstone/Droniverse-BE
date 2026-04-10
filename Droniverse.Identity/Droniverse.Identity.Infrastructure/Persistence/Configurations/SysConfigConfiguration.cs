using Droniverse.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Identity.Infrastructure.Persistence.Configurations;
public class SysConfigConfiguration : IEntityTypeConfiguration<SysConfig>
{
    public void Configure(EntityTypeBuilder<SysConfig> builder)
    {
        builder.ToTable("SysConfig");
        builder.HasKey(sc => sc.SysConfigID);

        builder.Property(sc => sc.Email)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(sc => sc.FacebookUrl)
            .HasColumnType("text")
            .IsRequired();
        builder.Property(sc => sc.PhoneNumber)
            .HasMaxLength(15)
            .IsRequired();
        builder.Property(sc => sc.LogoSystem)
            .HasColumnType("text").IsRequired();
        builder.Property(sc => sc.BufferEstimatedDuration)
            .HasColumnType("int")
            ;
        builder.Property(sc => sc.LogoCertificate)
            .HasColumnType("text").IsRequired();

        builder.Property(sc => sc.CertificateTemplateUrl).HasColumnType("text");

    }
}

