using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;

public class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        builder.ToTable("Competition");

        builder.HasKey(c => c.CompetitionID);

        builder.Property(c => c.CompetitionID)
            .HasColumnType("char(36)");

        builder.Property(c => c.ClubID)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.HasOne(c => c.Club)
            .WithMany(cl => cl.Competitions)
            .HasForeignKey(c => c.ClubID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.CreatedBy)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(c => c.UpdatedBy)
            .HasColumnType("char(36)");

        builder.Property(c => c.NameVN)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.NameEN)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.DescriptionVN)
            .HasColumnType("text");

        builder.Property(c => c.DescriptionEN)
            .HasColumnType("text");

        builder.Property(c => c.RuleContent)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(c => c.MaxParticipants)
            .HasColumnType("int");

        builder.Property(c => c.RegistrationStartDate)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(c => c.RegistrationEndDate)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(c => c.StartDate)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(c => c.EndDate)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.VisibleAt).HasColumnType("datetime").IsRequired();

        builder.Property(c => c.ResultPublishedAt)
            .HasColumnType("datetime");

        builder.Property(c => c.InvalidReason).HasColumnType("varchar(45)");
        builder.Property(c => c.InvalidAt).HasColumnType("datetime");

        builder.Property(c => c.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnType("datetime");
    }
}