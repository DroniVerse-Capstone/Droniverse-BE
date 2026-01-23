using Droniverse.Community.Domain.Entities.Mongo;
using Droniverse.Community.Infrastructure.Persistence.MongoDb.Configurations;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Droniverse.Community.Infrastructure.Persistence.MongoDb;
public class MongoDbContext : DbContext
{
    public MongoDbContext(DbContextOptions<MongoDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Invoice> Invoices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // CHỈ apply configurations có namespace chứa "MongoDb.Configurations"
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MongoDbContext).Assembly,
            type => type.Namespace != null &&
                    type.Namespace.Contains("MongoDb.Configurations"));

        modelBuilder.Entity<Order>().ToCollection("orders");
        modelBuilder.Entity<Invoice>().ToCollection("invoices");
    }

}

