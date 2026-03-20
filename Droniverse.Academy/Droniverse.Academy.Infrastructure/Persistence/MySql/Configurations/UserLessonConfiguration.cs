using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class UserLessonConfiguration : IEntityTypeConfiguration<UserLesson>
{
    public void Configure(EntityTypeBuilder<UserLesson> builder)
    {
        builder.ToTable("UserLesson");
        builder.HasKey(e => e.UserLessonID);

        builder.HasOne(e => e.Lesson)
            .WithMany(e => e.UserLessons)
            .HasForeignKey(e => e.LessonID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.UserLessonID).HasColumnType("char(36)");
        builder.Property(e => e.LessonID).HasColumnType("char(36)");
        builder.Property(e => e.UserID).HasColumnType("char(36)");
        builder.Property(e => e.Status)
            .HasColumnType("tinyint")
            .HasConversion<byte>()
            .HasDefaultValue(UserLessonStatus.INCOMPLETED)
            .IsRequired();
        builder.Property(e => e.Progress).HasColumnType("float");
        builder.Property(e => e.LastAccessDate).HasColumnType("datetime");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_UserLesson_Progress", "`Progress` BETWEEN 0 AND 100");
            t.HasCheckConstraint("CK_UserLesson_Status", "`Status` IN (0,1,2)");
        });
    }
}
