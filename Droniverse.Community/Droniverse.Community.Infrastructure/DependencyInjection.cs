using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.IRepository.Mongo;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Droniverse.Community.Infrastructure.Repositories;
using Droniverse.Community.Infrastructure.Repositories.Mongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Droniverse.Community.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //mysql

        //string connectionString = connectionStringTemplate
        //    .Replace("$MYSQL_HOST", Environment.GetEnvironmentVariable("MYSQL_HOST"))
        //    .Replace("$MYSQL_PORT", Environment.GetEnvironmentVariable("MYSQL_PORT"))
        //    .Replace("$MYSQL_DATABASE", Environment.GetEnvironmentVariable("MYSQL_DATABASE"))
        //    .Replace("$MYSQL_USER", Environment.GetEnvironmentVariable("MYSQL_USER"))
        //    .Replace("$MYSQL_PASSWORD", Environment.GetEnvironmentVariable("MYSQL_PASSWORD"));

        services.AddDbContext<MySqlDbContext>(options =>
        {
            options.UseMySQL(configuration.GetConnectionString("MySqlConnection"));
        });

        //mongodb
        var connectionString = configuration["MongoDbSettings:ConnectionString"];
        var databaseName = configuration["MongoDbSettings:DatabaseName"];
        services.AddSingleton<IMongoClient>(_ =>
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            return new MongoClient(settings);
        });
        services.AddScoped<IMongoDatabase>(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        return services;
    }
}

