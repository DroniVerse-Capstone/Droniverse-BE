using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class CourseVersionConfiguration : IEntityTypeConfiguration<CourseVersion>
{
    public void Configure(EntityTypeBuilder<CourseVersion> builder)
    {
        builder.ToTable("CourseVersion");

        builder.HasKey(c => c.CourseVersionID);
        builder.Property(c => c.CourseVersionID).HasColumnType("char(36)");

        builder.HasOne(cv => cv.Course)
            .WithMany(c => c.CourseVersions)
            .HasForeignKey(cv => cv.CourseID)
            .OnDelete(DeleteBehavior.Restrict); // xóa course thì k xóa course version
        builder.HasMany(cv => cv.CourseVersionCategories)
            .WithOne(cvc => cvc.CourseVersion);
        builder.HasMany(cv => cv.Codes)
            .WithOne(c => c.CourseVersion);
        builder.HasMany(cv => cv.Feedbacks)
            .WithOne(f => f.CourseVersion);
        builder.HasMany(cv => cv.Modules)
            .WithOne(f => f.CourseVersion);
        builder.HasMany(cv => cv.Codes)
            .WithOne(f => f.CourseVersion);
        builder.HasMany(cv => cv.RequiredDrones)
            .WithOne(f => f.CourseVersion);
        builder.HasMany(cv => cv.Enrollments)
            .WithOne(e => e.CourseVersion);


        builder.Property(c => c.CourseID).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.TitleVN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.TitleEN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.DescriptionVN).HasColumnType("text");
        builder.Property(c => c.DescriptionEN).HasColumnType("text");
        builder.Property(c => c.Status)
            .HasColumnType("tinyint")
            .IsRequired()
            //.HasDefaultValue(CourseStatus.ACTIVE)
            .HasConversion<byte>();

        builder.Property(c => c.Version).HasColumnType("int");
        builder.Property(c => c.ImageUrl).HasColumnType("text");
        builder.Property(c => c.Level)
            .HasMaxLength(10)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.EstimatedDuration)
            .HasColumnType("int");
        builder.Property(c => c.Version)
            .HasColumnType("int")
            //.HasDefaultValue("1")
            .IsRequired();
        builder.Property(c => c.UpdateBy)
            .HasColumnType("char(36)");
        builder.Property(c => c.UpdateAt)
            .HasColumnType("datetime")
            .ValueGeneratedOnUpdate();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_CourseVersion_Status", "`Status` IN (0,1,2,3)");
            t.HasCheckConstraint("CK_CourseVersion_Level", "`Level` IN ('EASY', 'MEDIUM', 'HARD')");
        });
    }
}

