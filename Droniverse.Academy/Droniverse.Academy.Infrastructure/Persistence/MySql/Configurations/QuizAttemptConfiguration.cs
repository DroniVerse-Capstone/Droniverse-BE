using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("QuizAttempt");
        builder.HasKey(e => e.AttemptID);

        builder.HasOne(e => e.Quiz)
            .WithMany(e => e.QuizAttempts)
            .HasForeignKey(e => e.QuizID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.QuizQuestionAttempts)
            .WithOne(e => e.QuizAttempt)
            .HasForeignKey(e => e.AttemptID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.AttemptID).HasColumnType("char(36)");
        builder.Property(e => e.QuizID).HasColumnType("char(36)");
        builder.Property(e => e.UserID).HasColumnType("char(36)");
        builder.Property(e => e.StartTime).HasColumnType("datetime").IsRequired();
        builder.Property(e => e.SubmitTime).HasColumnType("datetime");
        builder.Property(e => e.Score).HasColumnType("float");
        builder.Property(e => e.IsPassed).HasColumnType("tinyint(1)").HasDefaultValue(false);
    }
}
