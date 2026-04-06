using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("Round");

        builder.HasKey(r => r.RoundID);

        builder.Property(r => r.RoundID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(r => r.CompetitionID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(r => r.LabID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(r => r.RoundNumber)
            .HasColumnType("int")
            .IsRequired();

        builder.Property(r => r.StartTime)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(r => r.EndTime)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(r => r.TimeLimit)
          .HasColumnType("time")
          .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(r => r.IsSummarized)
            .HasColumnType("tinyint(1)")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnType("datetime");

        builder.Property(r => r.UpdatedBy)
            .HasColumnType("char(36)");

        builder.HasOne(r => r.Competition)
            .WithMany(c => c.Rounds)
            .HasForeignKey(r => r.CompetitionID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.UserRounds)
            .WithOne(ur => ur.Round)
            .HasForeignKey(ur => ur.RoundID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_Round_Status", "Status IN (0,1,2,3)")
        );
    }
}