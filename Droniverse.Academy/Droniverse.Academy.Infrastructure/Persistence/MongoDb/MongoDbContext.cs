using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using Mysqlx.Crud;

namespace Droniverse.Academy.Infrastructure.Persistence.MongoDb;

public class MongoDbContext : DbContext
{
    public MongoDbContext(DbContextOptions<MongoDbContext> options) : base(options) { }

    public DbSet<LabContent> LabContents { get; set; }
    public DbSet<SimulateConfig> SimulateConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CHỈ apply configurations có namespace chứa "MongoDb.Configurations"
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MongoDbContext).Assembly,
            type => type.Namespace != null &&
                    type.Namespace.Contains("MongoDb.Configurations"));

        modelBuilder.Entity<LabContent>().ToCollection("lab_contents");
        modelBuilder.Entity<SimulateConfig>().ToCollection("simulate_configs");
    }
}

