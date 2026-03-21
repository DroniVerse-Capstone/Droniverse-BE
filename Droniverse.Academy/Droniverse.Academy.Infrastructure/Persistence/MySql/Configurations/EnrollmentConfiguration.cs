using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollment");

        // Primary Key
        builder.HasKey(e => e.EnrollmentID);
        builder.Property(e => e.EnrollmentID)
            .HasColumnType("char(36)");

        // Foreign Keys
        builder.Property(e => e.CourseVersionID)
            .HasColumnType("char(36)")
            .IsRequired();
        builder.Property(e => e.UserID)
            .HasColumnType("char(36)")
            .IsRequired();
        builder.Property(e => e.ClubID)
            .HasColumnType("char(36)");

        // Properties
        builder.Property(e => e.EnrollDate)
            .HasColumnType("datetime")
            .ValueGeneratedOnAdd();
        builder.Property(e => e.LastAccessDate)
            .HasColumnType("datetime");
        builder.Property(e => e.ExpireDate)
            .HasColumnType("datetime");
        builder.Property(e => e.Progress)
            .HasColumnType("float")
            .HasDefaultValue(0);
        builder.Property(e => e.Status)
            .HasColumnType("tinyint")
            .HasConversion<byte>()
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.CourseVersion)
            .WithMany(cv => cv.Enrollments)
            .HasForeignKey(e => e.CourseVersionID)
            .OnDelete(DeleteBehavior.Restrict);

        // Constraints
        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_Enrollment_Status",
                "`Status` IN (0,1,2,3)"
            ));
    }
}
