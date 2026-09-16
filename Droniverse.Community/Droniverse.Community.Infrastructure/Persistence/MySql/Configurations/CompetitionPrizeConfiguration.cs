using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations
{
    public class CompetitionPrizeConfiguration : IEntityTypeConfiguration<CompetitionPrize>
    {
        public void Configure(EntityTypeBuilder<CompetitionPrize> builder)
        {
            builder.ToTable("CompetitionPrize");

            builder.HasKey(p => p.CompetitionPrizeID);

            builder.Property(p => p.CompetitionPrizeID)
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(p => p.CompetitionID)
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(p => p.TitleVN)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(p => p.TitleEN)
                .HasMaxLength(255);

            builder.Property(p => p.DescriptionVN)
                .HasColumnType("text");

            builder.Property(p => p.DescriptionEN)
                .HasColumnType("text");

            builder.Property(p => p.RewardType)
                .HasColumnType("tinyint")
                .IsRequired();

            builder.Property(p => p.RewardValueGiftVN)
                .HasMaxLength(255);

            builder.Property(p => p.RewardValueGiftEN)
                .HasMaxLength(255);

            builder.Property(p => p.RewardValueMoney)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.RankFrom)
                .IsRequired();

            builder.Property(p => p.RankTo)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .HasColumnType("datetime")
                .IsRequired();

            builder.Property(p => p.CreatedBy)
                .HasColumnType("char(36)")
                .IsRequired();

            builder.Property(p => p.UpdatedAt)
                .HasColumnType("datetime");

            builder.Property(p => p.UpdatedBy)
                .HasColumnType("char(36)");

            builder.HasOne(p => p.Competition)
                .WithMany(c => c.CompetitionPrizes)
                .HasForeignKey(p => p.CompetitionID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
