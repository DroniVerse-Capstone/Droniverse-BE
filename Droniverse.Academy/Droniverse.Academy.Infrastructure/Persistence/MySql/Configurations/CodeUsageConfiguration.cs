using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class CodeUsageConfiguration : IEntityTypeConfiguration<CodeUsage>
{
    public void Configure(EntityTypeBuilder<CodeUsage> builder)
    {
        builder.ToTable("CodeUsage");

        builder.HasKey(cu => new { cu.CodeID , cu.UserID});
        builder.Property(c => c.CodeID).HasColumnType("varchar(50)");
        builder.HasOne(c => c.Code)
            .WithMany(cv => cv.CodeUsages)
            .HasForeignKey(c => c.CodeID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.UserID).HasColumnType("char(36)");
        builder.Property(c => c.UsedDate).HasColumnType("datetime");
    }
}

