using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;

public class FlightSimulatorConfiguration : IEntityTypeConfiguration<FlightSimulator>
{
    public void Configure(EntityTypeBuilder<FlightSimulator> builder)
    {
        builder.ToTable("FlightSimulator");

        builder.HasKey(x => x.FlightID);

        builder.Property(x => x.FlightID)
            .HasColumnType("char(36)");

        builder.Property(x => x.ContentVN)
            .HasColumnType("text")
            .IsRequired(false);

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
                "CK_FlightSimulator_EstimatedTime",
                "`EstimatedTime` > 0"
            ));
    }
}
