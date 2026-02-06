using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Course");

        builder.HasKey(c => c.CourseID);
        builder.Property(c => c.CourseID).HasColumnType("char(36)");

        builder.HasMany(c => c.CourseVersions)
            .WithOne(cv => cv.Course);
        builder.HasMany(c => c.Enrollments)
            .WithOne(cv => cv.Course);
        //builder.HasMany(c => c.ClubCourses)
        //    .WithOne(cv => cv.Course);
        builder.HasOne(c => c.Certificate)
            .WithOne(cv => cv.Course);

        builder.Property(c => c.CreateBy).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.CreateAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(e => e.Status).HasColumnType("tinyint").HasConversion<byte>();
        builder.ToTable(t => t.HasCheckConstraint("CK_Course_Status", "`Status` IN (0,1,2,3)"));
    }
}

