using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.Mapper;
using Droniverse.Community.Application.Services;
using Microsoft.Extensions.DependencyInjection;
namespace Droniverse.Community.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddAutoMapper(typeof(ClubMappingProfile).Assembly);
        services.AddScoped<IClubService, ClubService>();
        return services;
    }
}

