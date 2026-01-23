using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Application.Mapper;
using Droniverse.Identity.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Droniverse.Identity.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddAutoMapper(typeof(AccountMappingProfile).Assembly);
        services.AddAutoMapper(typeof(PermissionMappingProfile).Assembly);
        return services;
    }
}
