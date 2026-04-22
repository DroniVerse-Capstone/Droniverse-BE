using Droniverse.Shared.Services;
using Droniverse.Shared.Services.IServices;
using Droniverse.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Droniverse.Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDistributedMemoryCache();

        services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName)
        );

        services.Configure<AppSettings>(
            configuration.GetSection(AppSettings.SectionName)
        );

        // Email Service
        services.Configure<SendGridSettings>(configuration.GetSection("SendGrid"));
        services.AddScoped<IEmailService, EmailService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddSingleton<IClock, ClockService>();

        return services;
    }
}

