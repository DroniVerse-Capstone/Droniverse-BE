using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Application.Mapper;
using Droniverse.Community.Application.Services;
using Droniverse.Community.Application.Services.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Droniverse.Community.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Chỉ cần thêm 1 mapping profile là đc
        services.AddAutoMapper(typeof(ClubMappingProfile).Assembly);
        services.AddScoped<IClubService, ClubService>();
        services.AddScoped<IOrderService, OrderService>();

        services.AddHttpClient<IdentityMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"http://{configuration["IdentityMicroserviceName"]}:{configuration["IdentityMicroservicePort"]}");
        });
        services.AddHttpClient<AcademyMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"http://{configuration["AcademyMicroserviceName"]}:{configuration["AcademyMicroservicePort"]}");
        });
        return services;
    }
}

