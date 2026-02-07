using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class CourseVersionConfiguration : IEntityTypeConfiguration<CourseVersion>
{
    public void Configure(EntityTypeBuilder<CourseVersion> builder)
    {
        builder.ToTable("CourseVersion");

        // Primary Key
        builder.HasKey(cv => cv.CourseVersionID);
        builder.Property(cv => cv.CourseVersionID)
            .HasColumnType("char(36)");

        // Foreign Keys
        builder.Property(cv => cv.CourseID)
            .HasColumnType("char(36)")
            .IsRequired();
        builder.Property(cv => cv.UpdateBy)
            .HasColumnType("char(36)")
            .IsRequired();

        // Properties
        builder.Property(cv => cv.TitleVN)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(cv => cv.TitleEN)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(cv => cv.DescriptionVN)
            .HasColumnType("text");
        builder.Property(cv => cv.DescriptionEN)
            .HasColumnType("text");
        builder.Property(cv => cv.Status)
            .HasColumnType("tinyint")
            .HasConversion<byte>()
            .IsRequired();
        builder.Property(cv => cv.Version)
            .HasColumnType("int")
            .IsRequired();
        builder.Property(cv => cv.ImageUrl)
            .HasColumnType("text");
        builder.Property(cv => cv.Level)
            .HasMaxLength(10)
            .HasConversion<string>()
            .IsRequired();
        builder.Property(cv => cv.EstimatedDuration)
            .HasColumnType("int");
        builder.Property(cv => cv.UpdateAt)
            .HasColumnType("datetime")
            .ValueGeneratedOnUpdate();

        // Relationships
        builder.HasOne(cv => cv.Course)
            .WithMany(c => c.CourseVersions)
            .HasForeignKey(cv => cv.CourseID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(cv => cv.Modules)
            .WithOne(m => m.CourseVersion)
            .HasForeignKey(m => m.CourseVersionID);

        builder.HasMany(cv => cv.CourseVersionCategories)
            .WithOne(cvc => cvc.CourseVersion)
            .HasForeignKey(cvc => cvc.CourseVersionID);

        builder.HasMany(cv => cv.Codes)
            .WithOne(c => c.CourseVersion)
            .HasForeignKey(c => c.CourseVersionID);

        builder.HasMany(cv => cv.Feedbacks)
            .WithOne(f => f.CourseVersion)
            .HasForeignKey(f => f.CourseVersionID);

        builder.HasMany(cv => cv.RequiredDrones)
            .WithOne(rd => rd.CourseVersion)
            .HasForeignKey(rd => rd.CourseVersionID);

        builder.HasMany(cv => cv.Enrollments)
            .WithOne(e => e.CourseVersion)
            .HasForeignKey(e => e.CourseVersionID);

        builder.HasOne(cv => cv.Certificate)
            .WithOne(c => c.CourseVersion)
            .HasForeignKey<Certificate>(c => c.CourseVersionID)
            .OnDelete(DeleteBehavior.Restrict);

        // Constraints
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_CourseVersion_Status",
                "`Status` IN (0,1,2,3)"
            );
            t.HasCheckConstraint(
                "CK_CourseVersion_Level",
                "`Level` IN ('EASY', 'MEDIUM', 'HARD')"
            );
        });
    }
}
