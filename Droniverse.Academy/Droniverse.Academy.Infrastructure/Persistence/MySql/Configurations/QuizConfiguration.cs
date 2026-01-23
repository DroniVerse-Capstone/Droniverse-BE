using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("Quiz");
        builder.HasKey(e => e.QuizID);

        builder.HasMany(e => e.QuizQuestions)
            .WithOne(c => c.Quiz);
        builder.HasOne(e => e.Lesson)
            .WithOne(c => c.Quiz)
            .HasForeignKey<Quiz>(e => e.LessonID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(e => e.QuizID).HasColumnType("char(36)");
        builder.Property(e => e.LessonID).HasColumnType("char(36)");
        builder.Property(e => e.TitleVN).HasColumnType("varchar(255)");
        builder.Property(e => e.TitleEN).HasColumnType("varchar(255)");
        builder.Property(e => e.DescriptionVN).HasColumnType("text");
        builder.Property(e => e.DescriptionEN).HasColumnType("text");
        builder.Property(e => e.TimeLimit).HasColumnType("int");
        builder.Property(e => e.TotalScore).HasColumnType("float");
        builder.Property(e => e.PassScore).HasColumnType("float");
        builder.Property(e => e.CreateBy).HasColumnType("char(36)");
        builder.Property(e => e.UpdateBy).HasColumnType("char(36)");
        builder.Property(e => e.CreateAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(e => e.UpdateAt).HasColumnType("datetime").ValueGeneratedOnUpdate();


    }
}
