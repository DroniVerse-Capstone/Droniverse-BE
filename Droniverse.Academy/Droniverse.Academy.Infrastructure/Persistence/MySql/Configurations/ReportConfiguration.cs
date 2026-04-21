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

        builder.Property(r => r.ReportID).HasColumnType("char(36)");
        builder.Property(r => r.ReferenceID).HasColumnType("char(36)").IsRequired();
        builder.Property(r => r.UserID).HasColumnType("char(36)").IsRequired();
        builder.Property(r => r.Content).HasColumnType("text").IsRequired();
        builder.Property(r => r.ResponseVN).HasColumnType("text");
        builder.Property(r => r.ResponseEN).HasColumnType("text");
    }
}

