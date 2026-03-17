using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lesson");
        builder.HasKey(e => e.LessonID);

        builder.HasMany(e => e.UserLessons)
            .WithOne(c => c.Lesson)
            .HasForeignKey(c => c.LessonID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Module)
            .WithMany(c => c.Lessons)
            .HasForeignKey(e => e.ModuleID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Lab)
            .WithOne(c => c.Lesson)
            .HasForeignKey<Lab>(e => e.LessonID)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Quiz)
            .WithOne(c => c.Lesson)
            .HasForeignKey<Quiz>(e => e.LessonID)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(l => l.Theory)
            .WithOne(c => c.Lesson)
            .HasForeignKey<Theory>(e => e.LessonID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.LessonID).HasColumnType("char(36)");
        builder.Property(e => e.ModuleID).HasColumnType("char(36)");
        builder.Property(e => e.ReferenceID).HasColumnType("char(36)");
        builder.Property(e => e.Type).HasMaxLength(20).HasConversion<string>();
        builder.ToTable(t => t.HasCheckConstraint("CK_Lesson_Type", "`Type` IN ('THEORY', 'QUIZ', 'LAB')"));


    }
}
