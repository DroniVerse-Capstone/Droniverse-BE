using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class CodeConfiguration : IEntityTypeConfiguration<Code>
{
    public void Configure(EntityTypeBuilder<Code> builder)
    {
        builder.ToTable("Code");

        // PK
        builder.HasKey(c => c.CodeID);
        builder.Property(c => c.CodeID)
            .HasColumnType("varchar(50)")
            .IsUnicode(false)
            .IsRequired();

        // Club
        builder.Property(c => c.ClubID)
            .HasColumnType("char(36)")
            .IsRequired();

        // Course FK
        builder.Property(c => c.CourseID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.HasOne(c => c.Course)
            .WithMany(c => c.Codes)
            .HasForeignKey(c => c.CourseID)
            .OnDelete(DeleteBehavior.Restrict);

        // Usage
        builder.Property(c => c.UsedByUserID)
            .HasColumnType("char(36)")
            .IsRequired(false);

        builder.Property(c => c.UsedDate)
            .HasColumnType("datetime(6)")
            .IsRequired(false);

        // Expire
        builder.Property(c => c.ExpireDate)
            .HasColumnType("datetime(6)")
            .IsRequired();

        // Status (tinyint: 0,1,2)
        builder.Property(c => c.Status)
            .HasColumnType("tinyint")
            .HasConversion<byte>()
            .IsRequired();

        // Audit
        builder.Property(c => c.CreatedAt)
            .HasColumnType("datetime(6)")
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasColumnType("char(36)")
            .IsRequired();

        // Check constraint đúng theo spec
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Code_Status",
                "`Status` IN (0,1,2)"
            );
        });
    }
}