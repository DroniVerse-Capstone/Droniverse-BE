using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificate");

        // Primary Key
        builder.HasKey(c => c.CertificateID);
        builder.Property(c => c.CertificateID)
            .HasColumnType("char(36)");

        // Foreign Key
        builder.Property(c => c.CourseVersionID)
            .HasColumnType("char(36)")
            .IsRequired();

        // Properties
        builder.Property(c => c.CreateBy)
            .HasColumnType("char(36)")
            .IsRequired();
        builder.Property(c => c.UpdateBy)
            .HasColumnType("char(36)");
        builder.Property(c => c.CertificateNameVN)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(c => c.CertificateNameEN)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(c => c.ImageUrl)
            .HasColumnType("text")
            .IsRequired();
        //builder.Property(c => c.LogoCertificate)
        //    .HasColumnType("text");
        //builder.Property(c => c.Description)
        //    .HasColumnType("text");
        //builder.Property(c => c.Signature)
        //    .HasColumnType("text");
        //builder.Property(c => c.AuthorName)
        //    .HasMaxLength(100);
        builder.Property(c => c.CreateAt)
            .HasColumnType("datetime");
        builder.Property(c => c.UpdateAt)
            .HasColumnType("datetime");

        // Relationships
        builder.HasOne(c => c.CourseVersion)
            .WithOne(cv => cv.Certificate)
            .HasForeignKey<Certificate>(c => c.CourseVersionID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
