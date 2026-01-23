using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.ToTable("Certificate");

        builder.HasKey(c => c.CertificateID);
        builder.HasOne(c => c.Course)
            .WithOne(cv => cv.Certificate)
            .HasForeignKey<Certificate>(c => c.CourseID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(c => c.CertificateID).HasColumnType("char(36)");
        builder.Property(c => c.CourseID).HasColumnType("char(36)");
        builder.Property(c => c.ImageUrl).HasColumnType("text");
        builder.Property(c => c.CertificateName).HasMaxLength(255);
        builder.Property(c => c.LogoCertificate).HasColumnType("text");
        builder.Property(c => c.Description).HasColumnType("text");
        builder.Property(c => c.Signature).HasColumnType("text");
        builder.Property(c => c.AuthorName).HasMaxLength(100);
    }
}

