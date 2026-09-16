using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class LevelConfiguration : IEntityTypeConfiguration<Level>
{
    public void Configure(EntityTypeBuilder<Level> builder)
    {
        builder.ToTable("Level");

        builder.HasKey(l => l.LevelID);

        builder.Property(l => l.LevelID)
            .HasColumnType("char(36)");

        builder.Property(l => l.DroneID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(l => l.LevelNumber)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(l => l.Name)
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder.Property(l => l.DescriptionVN)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(l => l.DescriptionEN)
            .HasColumnType("text")
            .IsRequired(false);

        builder.HasOne(l => l.Drone)
            .WithMany()
            .HasForeignKey(l => l.DroneID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.LevelCourseRequirements)
            .WithOne(lcr => lcr.Level)
            .HasForeignKey(lcr => lcr.LevelID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.UserLevels)
            .WithOne(ul => ul.Level)
            .HasForeignKey(ul => ul.LevelID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.DroneID, l.LevelNumber })
            .IsUnique();

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_Level_LevelNumber",
                "`LevelNumber` IN (1,2,3,4)"
            ));
    }
}
