using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class QuizQuestionAttemptConfiguration : IEntityTypeConfiguration<QuizQuestionAttempt>
{
    public void Configure(EntityTypeBuilder<QuizQuestionAttempt> builder)
    {
        builder.ToTable("QuizQuestionAttempt");
        builder.HasKey(e => e.AttemptAnswerID);

        builder.HasOne(e => e.QuizAttempt)
            .WithMany(e => e.QuizQuestionAttempts)
            .HasForeignKey(e => e.AttemptID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.QuizQuestion)
            .WithMany(e => e.QuizQuestionAttempts)
            .HasForeignKey(e => e.QuestionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.AttemptAnswerID).HasColumnType("char(36)");
        builder.Property(e => e.AttemptID).HasColumnType("char(36)");
        builder.Property(e => e.QuestionID).HasColumnType("char(36)");
        builder.Property(e => e.SelectedAnswer).HasColumnType("char(1)");
        builder.Property(e => e.IsCorrect).HasColumnType("tinyint(1)").HasDefaultValue(false);
        builder.Property(e => e.Score).HasColumnType("float");

        builder.ToTable(t => t.HasCheckConstraint("CK_QuizQuestionAttempt_SelectedAnswer", "`SelectedAnswer` IN ('A','B','C','D')"));
    }
}
