using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class CompetitionCertificateConfiguration : IEntityTypeConfiguration<CompetitionCertificate>
{
    public void Configure(EntityTypeBuilder<CompetitionCertificate> builder)
    {
        builder.ToTable("CompetitionCertificate");

        builder.HasKey(c => new { c.CompetitionID , c.CertificateID });
        builder.Property(c => c.CompetitionID).HasColumnType("char(36)");
        builder.HasOne(cp => cp.Competition)
            .WithMany(c => c.CompetitionCertificates)
            .HasForeignKey(cp => cp.CompetitionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.CertificateID).HasColumnType("char(36)");
        //builder.HasOne(cp => cp.Certificate)
        //    .WithMany(c => c.CompetitionCertificates)
        //    .HasForeignKey(cp => cp.CertificateID)
        //    .OnDelete(DeleteBehavior.Restrict);

    }
}

