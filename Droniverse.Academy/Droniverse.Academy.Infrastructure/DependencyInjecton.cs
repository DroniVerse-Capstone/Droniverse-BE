using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Common;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Droniverse.Academy.Infrastructure.Repositories;
using Droniverse.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Droniverse.Academy.Infrastructure
{
    public static class DependencyInjecton
    {
        public static IServiceCollection AddInfrastructure (this IServiceCollection services, IConfiguration configuration)
        {
            //mysql
            services.AddDbContext<MySqlDbContext>(options =>
            {
                options.UseMySQL(configuration.GetConnectionString("MySqlConnection"));
            });

            //mongodb
            var connectionString = configuration["MongoDbSettings:ConnectionString"];
            var databaseName = configuration["MongoDbSettings:DatabaseName"];
            services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
            services.AddScoped<IMongoDatabase>(provider =>
            {
                var client = provider.GetRequiredService<IMongoClient>();
                return client.GetDatabase(databaseName);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddHttpContextAccessor();

            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddSingleton<IClock, SystemClock>();
            return services;
        }
    }
}
