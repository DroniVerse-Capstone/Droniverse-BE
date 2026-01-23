using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class UserCompetitionConfiguration : IEntityTypeConfiguration<UserCompetion>
{
    public void Configure(EntityTypeBuilder<UserCompetion> builder)
    {
        builder.ToTable("UserCompetion");

        builder.HasKey(c => new {c.UserID, c.CompetitionID});
        builder.Property(c => c.UserID).HasColumnType("char(36)").IsRequired();
        builder.Property(c => c.CompetitionID).HasColumnType("char(36)").IsRequired();
        builder.HasOne(p => p.Competition)
            .WithMany(pc => pc.UserCompetitions)
            .HasForeignKey(p => p.CompetitionID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

