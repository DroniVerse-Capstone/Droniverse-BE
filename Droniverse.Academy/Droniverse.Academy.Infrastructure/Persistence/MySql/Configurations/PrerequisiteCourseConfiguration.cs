using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class PrerequisiteCourseConfiguration : IEntityTypeConfiguration<PrerequisiteCourse>
{
    public void Configure(EntityTypeBuilder<PrerequisiteCourse> builder)
    {
        builder.ToTable("PrerequisiteCourse");

        builder.HasKey(pc => new { pc.CourseID, pc.PrerequisiteCourseID });

        builder.Property(pc => pc.CourseID)
            .HasColumnType("char(36)");

        builder.Property(pc => pc.PrerequisiteCourseID)
            .HasColumnType("char(36)");

        builder.HasOne(pc => pc.Course)
            .WithMany()
            .HasForeignKey(pc => pc.CourseID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pc => pc.RequiredCourse)
            .WithMany()
            .HasForeignKey(pc => pc.PrerequisiteCourseID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_PrerequisiteCourse_SelfReference",
                "`CourseID` <> `PrerequisiteCourseID`"
            ));
    }
}
