using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.Configurations;
public class RequiredDroneConfiguration : IEntityTypeConfiguration<RequiredDrone>
{
    public void Configure(EntityTypeBuilder<RequiredDrone> builder)
    {
        builder.ToTable("RequiredDrone");

        builder.HasKey(c => new {c.CourseVersionID, c.DroneID});
        builder.HasOne(c => c.CourseVersion)
            .WithMany(cv => cv.RequiredDrones)
            .HasForeignKey(c => c.CourseVersionID)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.Drone)
            .WithMany(cv => cv.RequiredDrones)
            .HasForeignKey(c => c.DroneID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.CourseVersionID).HasColumnType("char(36)");
        builder.Property(c => c.DroneID).HasColumnType("char(36)");
    }
}

