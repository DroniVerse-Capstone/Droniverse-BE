using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class ClubCourseConfiguration : IEntityTypeConfiguration<ClubCourse>
{
    public void Configure(EntityTypeBuilder<ClubCourse> builder)
    {
        builder.ToTable("ClubCourse");

        builder.HasKey(c => new { c.ClubID, c.CourseID });
        builder.Property(c => c.ClubID).HasColumnType("char(36)");
        builder.HasOne(cp => cp.Club)
            .WithMany(c => c.ClubCourses)
            .HasForeignKey(cp => cp.ClubID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.CourseID).HasColumnType("char(36)");
        builder.Property(c => c.ProfitType)
            .HasConversion<byte>()
            .IsRequired();
        builder.Property(c => c.RemainingQuantity);
        builder.Property(c => c.TotalQuantity);
        //builder.HasOne(cp => cp.Course)
        //    .WithMany(c => c.ClubCourses)
        //    .HasForeignKey(cp => cp.CourseID)
        //    .OnDelete(DeleteBehavior.Restrict);

    }
}

