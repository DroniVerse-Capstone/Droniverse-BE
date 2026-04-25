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

        builder.Property(e => e.LessonID).HasColumnType("char(36)");
        builder.Property(e => e.ModuleID).HasColumnType("char(36)");
        builder.Property(e => e.OrderIndex).HasColumnType("int").IsRequired();
        builder.Property(e => e.ReferenceID).HasColumnType("char(36)");
        builder.Property(e => e.Type).HasMaxLength(20).HasConversion<string>();
        builder.ToTable(t => t.HasCheckConstraint("CK_Lesson_Type", "`Type` IN ('THEORY', 'QUIZ', 'LAB', 'PHYSIC', 'LAB_PHYSIC', 'VR', 'ASSIGNMENT')"));
        builder.ToTable(t => t.HasCheckConstraint("CK_Lesson_OrderIndex", "`OrderIndex` > 0"));
        builder.HasIndex(e => new { e.ModuleID, e.OrderIndex }).IsUnique();


    }
}
