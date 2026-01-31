using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollment");
        builder.HasKey(e => e.EnrollmentID);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CourseVersion)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseVersionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.EnrollmentID).HasColumnType("char(36)");
        builder.Property(e => e.CourseID).HasColumnType("char(36)");
        builder.Property(e => e.CourseVersionID).HasColumnType("char(36)");
        builder.Property(e => e.UserID).HasColumnType("char(36)");
        builder.Property(e => e.ClubID).HasColumnType("char(36)");
        builder.Property(e => e.EnrollDate).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(e => e.IsCompleted).HasColumnType("tinyint");
        builder.Property(e => e.ExpireDate).HasColumnType("datetime");
        builder.Property(e => e.LastAccessDate).HasColumnType("datetime");
        builder.Property(e => e.Status).HasColumnType("tinyint").HasConversion<byte>();
        builder.ToTable(t => t.HasCheckConstraint("CK_Enrollment_Status", "`Status` IN (0,1,2)"));


    }
}
