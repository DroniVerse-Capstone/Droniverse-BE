using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedback");

        builder.HasKey(f => f.FeedbackID);

        builder.HasOne(f => f.CourseVersion)
            .WithMany(cv => cv.Feedbacks)
            .HasForeignKey(f => f.CourseVersionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(f => f.FeedbackID).HasColumnType("char(36)");
        builder.Property(f => f.UserID).HasColumnType("char(36)");
        builder.Property(f => f.CourseVersionID).HasColumnType("char(36)");
        builder.Property(f => f.Content).HasColumnType("text");
        builder.Property(f => f.CreatedAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(f => f.Rating).HasColumnType("tinyint").HasConversion<byte>();
        builder.ToTable(t => t.HasCheckConstraint("CK_Feedback_Rating", "`Rating` IN (1, 5)"));

    }
}
