using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class UserCertificateConfiguration : IEntityTypeConfiguration<UserCertificate>
{
    public void Configure(EntityTypeBuilder<UserCertificate> builder)
    {
        builder.ToTable("UserCertificate");

        builder.HasKey(uc => new {uc.UserID, uc.CertificateID});
        builder.HasOne(uc => uc.Certificate)
            .WithMany(c => c.UserCertificates)
            .HasForeignKey(c => c.CertificateID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(c => c.CertificateID).HasColumnType("char(36)");
        builder.Property(c => c.UserID).HasColumnType("char(36)");
        builder.Property(c => c.SerialNumber).HasColumnType("char(36)");
        builder.Property(c => c.AchievedDate).HasColumnType("date");
        builder.Property(c => c.Status).HasColumnType("bit").HasConversion<byte>();
        builder.ToTable(t => t.HasCheckConstraint("CK_UserCertificate_Status", "`Status` IN (0, 1)"));
        
    }
}

