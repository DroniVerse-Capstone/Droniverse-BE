using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class CourseVersionConfiguration
    : IEntityTypeConfiguration<CourseVersion>
{
    public void Configure(EntityTypeBuilder<CourseVersion> builder)
    {
        builder.ToTable("CourseVersion");

        /* =========================
           PRIMARY KEY
           ========================= */

        builder.HasKey(cv => cv.CourseVersionID);

        builder.Property(cv => cv.CourseVersionID)
            .HasColumnType("char(36)");

        /* =========================
           FOREIGN KEYS
           ========================= */

        builder.Property(cv => cv.CourseID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(cv => cv.UpdateBy)
            .HasColumnType("char(36)")   // nullable theo entity mới
            .IsRequired(false);

        /* =========================
           BASIC PROPERTIES
           ========================= */

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

        builder.Property(cv => cv.ContextVN)
            .HasColumnType("text");

        builder.Property(cv => cv.ContextEN)
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

        builder.Property(cv => cv.EstimatedDuration)
            .HasColumnType("int");

        builder.Property(cv => cv.ChangeLog)
            .HasColumnType("text");

        builder.Property(cv => cv.UpdateAt)
            .HasColumnType("datetime")
            .IsRequired(false); // vì nullable

        /* =========================
           RELATIONSHIPS
           ========================= */

        builder.HasOne(cv => cv.Course)
            .WithMany(c => c.CourseVersions)
            .HasForeignKey(cv => cv.CourseID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(cv => cv.Modules)
            .WithOne(m => m.CourseVersion)
            .HasForeignKey(m => m.CourseVersionID);

        builder.HasMany(cv => cv.Feedbacks)
            .WithOne(f => f.CourseVersion)
            .HasForeignKey(f => f.CourseVersionID);

        builder.HasMany(cv => cv.Enrollments)
            .WithOne(e => e.CourseVersion)
            .HasForeignKey(e => e.CourseVersionID);

        builder.HasOne(cv => cv.Certificate)
            .WithOne(c => c.CourseVersion)
            .HasForeignKey<Certificate>(c => c.CourseVersionID)
            .OnDelete(DeleteBehavior.Restrict);

        /* =========================
           INDEXES
           ========================= */

        // Unique Version per Course
        builder.HasIndex(cv => new { cv.CourseID, cv.Version })
            .IsUnique();


        /* =========================
           CHECK CONSTRAINTS
           ========================= */

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_CourseVersion_Status",
                "`Status` IN (0,1,2,3)"
            );

        });
    }
}