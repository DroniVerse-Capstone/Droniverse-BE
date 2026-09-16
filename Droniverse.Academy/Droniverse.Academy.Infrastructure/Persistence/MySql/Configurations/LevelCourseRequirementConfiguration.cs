using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class LevelCourseRequirementConfiguration : IEntityTypeConfiguration<LevelCourseRequirement>
{
    public void Configure(EntityTypeBuilder<LevelCourseRequirement> builder)
    {
        builder.ToTable("LevelCourseRequirement");

        builder.HasKey(lcr => new { lcr.LevelID, lcr.CourseID });

        builder.Property(lcr => lcr.LevelID)
            .HasColumnType("char(36)");

        builder.Property(lcr => lcr.CourseID)
            .HasColumnType("char(36)");

        builder.HasOne(lcr => lcr.Level)
            .WithMany(l => l.LevelCourseRequirements)
            .HasForeignKey(lcr => lcr.LevelID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(lcr => lcr.Course)
            .WithMany()
            .HasForeignKey(lcr => lcr.CourseID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
