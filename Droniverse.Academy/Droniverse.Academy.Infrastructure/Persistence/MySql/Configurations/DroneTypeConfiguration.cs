using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class DroneTypeConfiguration : IEntityTypeConfiguration<DroneType>
{
    public void Configure(EntityTypeBuilder<DroneType> builder)
    {
        builder.ToTable("DroneType");

        builder.HasKey(c => c.DroneTypeID);

        builder.HasMany(dt => dt.Drones)
               .WithOne(d => d.DroneType);
        
        builder.Property(c => c.DroneTypeID).HasColumnType("char(36)");
        builder.Property(c => c.TypeNameEN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.TypeNameVN).HasMaxLength(255).IsRequired();
        builder.Property(d => d.DescriptionVN).HasColumnType("text");
        builder.Property(d => d.DescriptionEN).HasColumnType("text");
      
    }
}

