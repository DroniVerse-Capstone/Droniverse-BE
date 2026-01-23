using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Report");

        builder.HasKey(r => r.ReportID);

        builder.HasOne(r => r.Lab)
               .WithMany(l => l.Reports)
               .HasForeignKey(r => r.LabID)
               .OnDelete(DeleteBehavior.Restrict);
        builder.Property(r => r.ReportID).HasColumnType("char(36)");
        builder.Property(r => r.LabID).HasColumnType("char(36)").IsRequired();
        builder.Property(r => r.UserID).HasColumnType("char(36)").IsRequired();
        builder.Property(r => r.Content).HasColumnType("text").IsRequired();
        builder.Property(r => r.ResponseVN).HasColumnType("text");
        builder.Property(r => r.ResponseEN).HasColumnType("text");
    }
}

