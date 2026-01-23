using Droniverse.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Droniverse.Identity.Infrastructure.Persistence.Configurations;
public class RolePermissConfiguration : IEntityTypeConfiguration<RolePermission>
{
    void IEntityTypeConfiguration<RolePermission>.Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermission");
        builder.HasKey(rp => new { rp.RoleID, rp.PermissionID });

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
