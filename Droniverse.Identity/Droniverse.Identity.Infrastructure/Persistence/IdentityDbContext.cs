using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Identity.Infrastructure.Persistence;
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {

    }
    public DbSet<Role> Role { get; set; }
    public DbSet<Permission> Permission { get; set; }
    public DbSet<Account> Account { get; set; }
    public DbSet<UserInfo> UserInfo { get; set; }
    public DbSet<RolePermission> RolePermission { get; set; }
    public DbSet<UserConfig> UserConfig { get; set; }
    public DbSet<SysConfig> SysConfig { get; set; }
    public DbSet<Notification> Notification { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissConfiguration());
        modelBuilder.ApplyConfiguration(new UserInfoConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfigConfiguration());
        modelBuilder.ApplyConfiguration(new SysConfigConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
