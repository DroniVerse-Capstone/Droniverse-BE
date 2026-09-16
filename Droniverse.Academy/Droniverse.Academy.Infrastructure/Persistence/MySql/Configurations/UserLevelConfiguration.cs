using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class UserLevelConfiguration : IEntityTypeConfiguration<UserLevel>
{
    public void Configure(EntityTypeBuilder<UserLevel> builder)
    {
        builder.ToTable("UserLevel");

        builder.HasKey(ul => ul.UserLevelID);

        builder.Property(ul => ul.UserLevelID)
            .HasColumnType("char(36)");

        builder.Property(ul => ul.UserID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(ul => ul.LevelID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(ul => ul.AchievedAt)
            .HasColumnType("datetime")
            .IsRequired(false);

        builder.HasOne(ul => ul.Level)
            .WithMany(l => l.UserLevels)
            .HasForeignKey(ul => ul.LevelID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ul => new { ul.UserID, ul.LevelID })
            .IsUnique();
    }
}
