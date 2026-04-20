using Droniverse.Community.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
public class ClubPolicyConfiguration : IEntityTypeConfiguration<ClubPolicy>
{
    public void Configure(EntityTypeBuilder<ClubPolicy> builder)
    {
        builder.ToTable("ClubPolicy");
        builder.HasKey(cp => cp.ClubPolicyID);
        builder.Property(cp => cp.ClubPolicyID).HasColumnType("char(36)");

        builder.Property(cp => cp.ClubID).IsRequired().HasColumnType("char(36)");
        builder.Property(cp => cp.Title).IsRequired().HasMaxLength(255);
        builder.Property(cp => cp.Content).IsRequired().HasColumnType("text");
        builder.Property(cp => cp.CreatedAt).HasColumnType("datetime").ValueGeneratedOnAdd();
        builder.Property(cp => cp.UpdatedAt).HasColumnType("datetime").ValueGeneratedOnAddOrUpdate();
        builder.Property(cp => cp.CreatedBy).HasColumnType("char(36)");
        builder.Property(cp => cp.UpdatedBy).HasColumnType("char(36)");
    }
}

