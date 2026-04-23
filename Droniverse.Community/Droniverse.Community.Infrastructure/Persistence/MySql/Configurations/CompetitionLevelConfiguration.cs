using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations
{
    public class CompetitionLevelConfiguration : IEntityTypeConfiguration<CompetitionLevel>
    {
        public void Configure(EntityTypeBuilder<CompetitionLevel> builder)
        {
            builder.ToTable("CompetitionLevel");

            // Composite key
            builder.HasKey(cl => new { cl.CompetitionID, cl.LevelID });

            builder.Property(cl => cl.CompetitionID)
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(cl => cl.LevelID)
                .HasColumnType("char(36)")
                .IsRequired();

            // Relationship với Competition
            builder.HasOne(cl => cl.Competition)
                .WithMany(c => c.CompetitionLevels) 
                .HasForeignKey(cl => cl.CompetitionID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(cl => cl.LevelID);
        }
    }
}