using Droniverse.Academy.Application.Delegate;
using Droniverse.Identity.Application.HttpClients;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Application.Mapper;
using Droniverse.Identity.Application.RabbitMQ;
using Droniverse.Identity.Application.Services;
using Droniverse.Shared.Messages;
using Droniverse.Shared.Messages.User;
using Droniverse.Shared.Services;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Droniverse.Identity.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
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

        services.AddTransient<AuthorizationDelegatingHandler>();

        //đăng ký httpclient
        services.AddHttpClient<AcademyMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"http://{configuration["AcademyMicroserviceName"]}:{configuration["AcademyMicroservicePort"]}");
        }).AddHttpMessageHandler<AuthorizationDelegatingHandler>(); ;
        services.AddHttpClient<CommunityMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"http://{configuration["CommunityMicroserviceName"]}:{configuration["CommunityMicroservicePort"]}");
        }).AddHttpMessageHandler<AuthorizationDelegatingHandler>();

        // Đăng ký Redis
        services.AddStackExchangeRedisCache(options =>
        {
            var host = configuration["Redis:Host"];
            var port = configuration["Redis:Port"];
            var password = configuration["Redis:Password"];
            var user = configuration["Redis:User"];
            options.Configuration = $"{host}:{port},password={password},user={user}";
        });

        return services;
    }
}
