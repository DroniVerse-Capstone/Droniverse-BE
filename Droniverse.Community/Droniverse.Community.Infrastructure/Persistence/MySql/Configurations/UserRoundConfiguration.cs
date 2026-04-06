using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class UserRoundConfiguration : IEntityTypeConfiguration<UserRound>
{
    public void Configure(EntityTypeBuilder<UserRound> builder)
    {
        builder.ToTable("UserRound");

        builder.HasKey(ur => ur.UserRoundID);

        builder.Property(ur => ur.UserRoundID).HasColumnType("char(36)");
        builder.Property(ur => ur.UserID).HasColumnType("char(36)").IsRequired();
        builder.Property(ur => ur.RoundID).HasColumnType("char(36)").IsRequired();

        builder.HasIndex(ur => ur.RoundID);

        builder.HasOne(ur => ur.Round)
            .WithMany(r => r.UserRounds)
            .HasForeignKey(ur => ur.RoundID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(ur => ur.Solution).HasColumnType("text");

        builder.Property(ur => ur.Status)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(ur => ur.ExecutionTime)
            .HasColumnType("time");

        builder.Property(ur => ur.NumberOfSteps)
            .HasColumnType("int");

        builder.Property(ur => ur.PathLength)
            .HasColumnType("float");

        builder.Property(ur => ur.FeedbackVN)
            .HasColumnType("text");

        builder.Property(ur => ur.FeedbackEN)
            .HasColumnType("text");

        builder.Property(ur => ur.Rating)
            .HasColumnType("int");

        builder.Property(ur => ur.Rank).HasColumnType("int");

        builder.Property(ur => ur.Point)
            .HasColumnType("decimal(18,2)");

        builder.Property(ur => ur.SubmittedAt)
            .HasColumnType("datetime");

        builder.Property(ur => ur.StartedAt)
            .HasColumnType("datetime");

        builder.Property(ur => ur.IsPassed)
            .HasColumnType("tinyint(1)");

        builder.Property(ur => ur.IsSequentialCheckpoints)
            .HasColumnType("tinyint(1)");
    }
}

