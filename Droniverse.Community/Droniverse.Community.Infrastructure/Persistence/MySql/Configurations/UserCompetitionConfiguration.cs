using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;

public class UserCompetitionConfiguration : IEntityTypeConfiguration<UserCompetition>
{
    public void Configure(EntityTypeBuilder<UserCompetition> builder)
    {
        builder.ToTable("User_Competition");

        builder.HasKey(x => x.UserCompetitionID);

        builder.Property(x => x.UserCompetitionID)
            .HasColumnType("char(36)");

        builder.Property(x => x.UserID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(x => x.CompetitionID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.HasOne(x => x.Competition)
            .WithMany(c => c.UserCompetitions)
            .HasForeignKey(x => x.CompetitionID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Score)
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.Rank)
            .HasColumnType("int");

        builder.Property(x => x.PrizeID)
            .HasColumnType("char(36)");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("datetime");

        builder.HasIndex(x => x.CompetitionID);

        builder.HasIndex(x => x.UserID);
    }
}