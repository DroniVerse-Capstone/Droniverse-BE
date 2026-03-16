using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Course");

        // Primary Key
        builder.HasKey(c => c.CourseID);
        builder.Property(c => c.CourseID)
            .HasColumnType("char(36)");

        // Properties
        builder.Property(c => c.CreateBy)
            .HasColumnType("char(36)")
            .IsRequired();
        builder.Property(c => c.CreateAt)
            .HasColumnType("datetime")
            .ValueGeneratedOnAdd();
        builder.Property(c => c.Status)
            .HasColumnType("tinyint")
            .HasConversion<byte>()
            .IsRequired();
        builder.Property(c => c.CurrentVersionID)
            .HasColumnType("char(36)")
            .IsRequired(false);

        // Relationships
        builder.HasOne(c => c.CurrentVersion)
            .WithMany()
            .HasForeignKey(c => c.CurrentVersionID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(c => c.CourseVersions)
            .WithOne(cv => cv.Course)
            .HasForeignKey(cv => cv.CourseID)
            .OnDelete(DeleteBehavior.Restrict);


        // Constraints
        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_Course_Status",
                "`Status` IN (0,1,2,3)"
            ));
    }
}
