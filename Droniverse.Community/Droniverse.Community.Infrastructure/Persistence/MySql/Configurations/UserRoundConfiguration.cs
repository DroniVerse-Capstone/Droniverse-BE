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
        builder.Property(c => c.UserID).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.RoundID).HasColumnType("char(36)").IsRequired();
        builder.HasOne(p => p.Round)
            .WithMany(pc => pc.UserRounds)
            .HasForeignKey(p => p.RoundID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(ur => ur.Solution).HasColumnType("text");
        builder.Property(ur => ur.IsCompleted).HasColumnType("tinyint(1)").IsRequired();
        builder.Property(ur => ur.Time).HasColumnType("float");
        builder.Property(ur => ur.NumberOfStep).HasColumnType("int");
        builder.Property(ur => ur.Length).HasColumnType("float");
        builder.Property(ur => ur.FeedbackVN).HasColumnType("text");
        builder.Property(ur => ur.FeedbackEN).HasColumnType("text");
        builder.Property(ur => ur.Rating).HasColumnType("int");
        builder.Property(ur => ur.Point).HasColumnType("decimal(18,2)");
    }
}

