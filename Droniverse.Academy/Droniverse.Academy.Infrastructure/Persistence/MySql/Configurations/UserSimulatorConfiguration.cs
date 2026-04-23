using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations
{
    public class UserSimulatorConfiguration : IEntityTypeConfiguration<UserSimulator>
    {
        public void Configure(EntityTypeBuilder<UserSimulator> builder)
        {
            builder.ToTable("UserSimulator");
            builder.HasKey(e => e.UserSimulatorID);

            builder.Property(e => e.UserSimulatorID).HasColumnType("char(36)");
            builder.Property(e => e.UserID).HasColumnType("char(36)");
            builder.Property(e => e.LessonID).HasColumnType("char(36)");
            builder.Property(e => e.FlightTime).HasColumnType("int");
            builder.Property(e => e.Score).HasColumnType("int");
            builder.Property(e => e.IsSuccess).HasColumnType("bit");

            builder.HasOne(e => e.Lesson)
                .WithMany()
                .HasForeignKey(e => e.LessonID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(e => new { e.UserID, e.LessonID });
        }
    }
}
