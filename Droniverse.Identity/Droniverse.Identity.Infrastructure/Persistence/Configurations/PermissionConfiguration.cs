using Droniverse.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Droniverse.Identity.Infrastructure.Persistence.Configurations;
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permission");
        builder.HasKey(p => p.PermissionID);

        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission);
        
        builder.Property(p => p.PermissionName)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(p => p.Description)
            .HasColumnType("text")
            .IsRequired(false);
    }
}

