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
        builder.Property(c => c.CompetitionID).HasColumnType("char(36)");
        builder.HasOne(cp => cp.Club)
            .WithMany(c => c.Competitions)
            .HasForeignKey(cp => cp.ClubID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.CreateBy).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.NameVN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.NameEN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.DescriptionEN).HasColumnType("text");
        builder.Property(c => c.DescriptionVN).HasColumnType("text");
        builder.Property(c => c.StartDate).HasColumnType("datetime").IsRequired();
        builder.Property(c => c.EndDate).HasColumnType("datetime").IsRequired();
    }
}

