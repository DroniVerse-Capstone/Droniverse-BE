using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
{
    public void Configure(EntityTypeBuilder<QuizAnswer> builder)
    {
        builder.ToTable("QuizAnswer");
        builder.HasKey(e => e.AnswerID);

        builder.HasOne(e => e.QuizQuestion)
            .WithMany(c => c.QuizAnswers)
            .HasForeignKey(qq => qq.QuestionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.AnswerID).HasColumnType("char(36)");
        builder.Property(e => e.QuestionID).HasColumnType("char(36)");
        builder.Property(e => e.ContentVN).HasColumnType("varchar(255)");
        builder.Property(e => e.ContentEN).HasColumnType("varchar(255)");
        builder.Property(e => e.IsCorrect).HasColumnType("tinyint(1)");



    }
}
