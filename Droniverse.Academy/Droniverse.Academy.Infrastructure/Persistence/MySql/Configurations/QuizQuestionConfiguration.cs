using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("QuizQuestion");
        builder.HasKey(e => e.QuestionID);

        builder.HasOne(e => e.Quiz)
            .WithMany(c => c.QuizQuestions)
            .HasForeignKey(qq => qq.QuizID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(e => e.QuizQuestionAttempts)
            .WithOne(c => c.QuizQuestion)
            .HasForeignKey(c => c.QuestionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.QuestionID).HasColumnType("char(36)");
        builder.Property(e => e.QuizID).HasColumnType("char(36)");
        builder.Property(e => e.ContentVN).HasColumnType("text");
        builder.Property(e => e.ContentEN).HasColumnType("text");
        builder.Property(e => e.AnswerA).HasColumnType("text");
        builder.Property(e => e.AnswerB).HasColumnType("text");
        builder.Property(e => e.AnswerC).HasColumnType("text");
        builder.Property(e => e.AnswerD).HasColumnType("text");
        builder.Property(e => e.AnswerA_EN).HasColumnType("text");
        builder.Property(e => e.AnswerB_EN).HasColumnType("text");
        builder.Property(e => e.AnswerC_EN).HasColumnType("text");
        builder.Property(e => e.AnswerD_EN).HasColumnType("text");
        builder.Property(e => e.CorrectAnswer).HasColumnType("char(1)");
        builder.Property(e => e.Score).HasColumnType("float");
        builder.ToTable(t => t.HasCheckConstraint("CK_QuizQuestion_CorrectAnswer", "`CorrectAnswer` IN ('A','B','C','D')"));



    }
}
