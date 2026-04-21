using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class WebSimulatorConfiguration : IEntityTypeConfiguration<WebSimulator>
{
    public void Configure(EntityTypeBuilder<WebSimulator> builder)
    {
        builder.ToTable("WebSimulator");

        builder.HasKey(x => x.WebSimulatorID);

        builder.Property(x => x.WebSimulatorID)
            .HasColumnType("char(36)");

        builder.Property(x => x.TitleEN)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.TitleVN)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
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
                "CK_WebSimulator_EstimatedTime",
                "`EstimatedTime` > 0"
            ));

        builder.ToTable(t =>
            t.HasCheckConstraint(
                "CK_WebSimulator_Type",
                "`Type` IN ('Physic', 'LabPhysic')"
            ));
    }
}
