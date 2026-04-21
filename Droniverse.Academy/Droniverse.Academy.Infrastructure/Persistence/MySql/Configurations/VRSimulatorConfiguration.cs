using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class VRSimulatorConfiguration : IEntityTypeConfiguration<VRSimulator>
{
    public void Configure(EntityTypeBuilder<VRSimulator> builder)
    {
        builder.ToTable("VRSimulator");

        builder.HasKey(x => x.VRSimulatorID);

        builder.Property(x => x.VRSimulatorID)
            .HasColumnType("char(36)");

        builder.Property(x => x.TitleEN)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.TitleVN)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.CreateBy)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(x => x.UpdateBy)
            .HasColumnType("char(36)")
            .IsRequired();

        builder.Property(x => x.CreateAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(x => x.UpdateAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.Property(x => x.EstimatedTime)
            .HasColumnType("int")
            .IsRequired();

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_VRSimulator_EstimatedTime",
                "`EstimatedTime` > 0"
            ));
    }
}
