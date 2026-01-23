using Droniverse.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Identity.Infrastructure.Persistence.Configurations;
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role");
        builder.HasKey(r => r.RoleID);

        builder.HasMany(r => r.Accounts)
            .WithOne(a => a.Role);

        builder.HasMany(r => r.RolePermissions)
            .WithOne(rp => rp.Role);

        builder.Property(r => r.RoleName)
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(r => r.Description)
            .HasColumnType("text")
            .IsRequired(false);
    }
}
