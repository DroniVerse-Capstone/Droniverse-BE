using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class DroneConfiguration : IEntityTypeConfiguration<Drone>
{
    public void Configure(EntityTypeBuilder<Drone> builder)
    {
        builder.ToTable("Drone");

        builder.HasKey(c => c.DroneID);
        builder.HasOne(d => d.DroneType)
               .WithMany(dt => dt.Drones)
               .HasForeignKey(d => d.DroneTypeID)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.DroneID).HasColumnType("char(36)");
        builder.Property(c => c.DroneTypeID).HasColumnType("char(36)");
        builder.Property(c => c.DroneNameVN).HasMaxLength(255).IsRequired();
        builder.Property(c => c.DroneNameEN).HasMaxLength(255).IsRequired();
        builder.Property(d => d.Manufacturer).HasMaxLength(100);
        builder.Property(d => d.DescriptionVN).HasColumnType("text");
        builder.Property(d => d.DescriptionEN).HasColumnType("text");
        builder.Property(d => d.Height).HasColumnType("float");
        builder.Property(d => d.Weight).HasColumnType("float");
        builder.Property(d => d.Status).HasColumnType("tinyint");
        builder.Property(d => d.ImgURL)
            .HasColumnName("ImgURL")
            .HasColumnType("char(255)");

        builder.ToTable(t => t.HasCheckConstraint("CK_Drone_Status", "`Status` IN (1,2,3)"));
      
    }
}

