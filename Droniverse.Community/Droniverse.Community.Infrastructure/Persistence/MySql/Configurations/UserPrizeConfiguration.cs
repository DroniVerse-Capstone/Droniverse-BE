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
    public class UserPrizeConfiguration : IEntityTypeConfiguration<UserPrize>
    {
        public void Configure(EntityTypeBuilder<UserPrize> builder)
        {
            builder.ToTable("UserPrize");

            builder.HasKey(x => x.UserPrizeID);

            builder.Property(x => x.UserPrizeID)
                .HasColumnName("userPrizeID")
                .IsRequired();

            builder.Property(x => x.UserID)
                .HasColumnName("userID")
                .IsRequired();

            builder.Property(x => x.PrizeID)
                .HasColumnName("prizeID")
                .IsRequired();

            builder.Property(x => x.Rank)
                .HasColumnName("rank")
                .IsRequired();

            builder.Property(x => x.RewardType)
                .HasColumnName("rewardType")
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.RewardValueMoney)
                .HasColumnName("rewardValueMoney")
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.RewardValueGiftVN)
                .HasColumnName("rewardValueGiftVN")
                .HasMaxLength(255);

            builder.Property(x => x.RewardValueGiftEN)
                .HasColumnName("rewardValueGiftEN")
                .HasMaxLength(255);

            builder.Property(x => x.IsAwarded)
                .HasColumnName("isAwarded")
                .IsRequired();

            builder.Property(x => x.AwardedAt)
                .HasColumnName("awardedAt");

            builder.Property(x => x.CreatedAt)
                .HasColumnName("createdAt")
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .HasColumnName("createdBy")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");

            builder.Property(x => x.UpdatedBy)
                .HasColumnName("updatedBy");

            builder.HasOne(x => x.Prize)
                .WithMany(p => p.UserPrizes)
                .HasForeignKey(x => x.PrizeID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
