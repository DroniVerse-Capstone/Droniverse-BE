using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class UserAttemptConfiguration : IEntityTypeConfiguration<UserAttempt>
{
    public void Configure(EntityTypeBuilder<UserAttempt> builder)
    {
        builder.ToTable("UserAttempt");

        builder.HasKey(ul => ul.AttemptID);

        builder.HasOne(um => um.Lesson)
            .WithMany(m => m.UserAttempts)
            .HasForeignKey(um => um.LessonID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(um => um.AttemptID).HasColumnType("char(36)");
        builder.Property(um => um.UserID).HasColumnType("char(36)");
        builder.Property(um => um.LessonID).HasColumnType("char(36)");
        builder.Property(um => um.AttemptTime).HasColumnType("int");
        builder.Property(um => um.IsCompleted).HasColumnType("tinyint(1)");



    }
}
