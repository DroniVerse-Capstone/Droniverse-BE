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
        builder.HasMany(e => e.QuizAnswers)
            .WithOne(c => c.QuizQuestion);

        builder.Property(e => e.QuestionID).HasColumnType("char(36)");
        builder.Property(e => e.QuizID).HasColumnType("char(36)");
        builder.Property(e => e.ContentVN).HasColumnType("varchar(255)");
        builder.Property(e => e.ContentEN).HasColumnType("varchar(255)");
        builder.Property(e => e.Type).HasColumnType("varchar(30)").HasConversion<string>();
        builder.Property(e => e.Score).HasColumnType("float");
        builder.ToTable(t => t.HasCheckConstraint("CK_Lab_Type", "`Type` IN ('MULTIPLE_CHOICE', 'TRUE_FALSE')"));



    }
}
