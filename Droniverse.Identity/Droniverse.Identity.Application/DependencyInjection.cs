using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Application.Mapper;
using Droniverse.Identity.Application.RabbitMQ;
using Droniverse.Identity.Application.Services;
using Droniverse.Shared.Messages;
using Droniverse.Shared.Messages.User;
using Droniverse.Shared.Services;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.DependencyInjection;

namespace Droniverse.Identity.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<ISysConfigService, SysConfigService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddAutoMapper(typeof(AccountMappingProfile).Assembly);
        services.AddAutoMapper(typeof(PermissionMappingProfile).Assembly);

        //rabbitmq
        services.AddTransient<IPublisher, UserPublisher>();
        services.AddTransient<IUserPublisher, UserPublisher>();
        
        // Đăng ký RabbitMQ Consumer cho notification
        services.AddSingleton<OrderNotificationConsumer>();

        return services;
    }
}
