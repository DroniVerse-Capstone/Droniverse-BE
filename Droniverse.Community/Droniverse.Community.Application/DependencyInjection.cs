using Droniverse.Community.Application.Delegate;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Application.Job;
using Droniverse.Community.Application.Jobs;

//using Droniverse.Community.Application.Jobs;
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
        services.AddAutoMapper(typeof(ClubRequestMappingProfile).Assembly);
        services.AddAutoMapper(typeof(CategoryMappingProfile).Assembly);
        services.AddScoped<IClubService, ClubService>();
        services.AddScoped<IClubAttemptRequestService, ClubAttemptRequestService>();
        services.AddScoped<IClubCreationRequestService, ClubCreationRequestService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICompetitionService, CompetitionService>();
        services.AddScoped<ICompetitionCertificateService, CompetitionCertificateService>();
        services.AddScoped<IRoundService, RoundService>();
        services.AddScoped<ICompetitionPrizeService, CompetitionPrizeService>();
        services.AddScoped<IUserRoundService, UserRoundService>();
        //services.AddScoped<IPaymentService, PaymentService>();

        // Đăng ký Background Jobs
        services.AddScoped<CompetitionStatusJob>();
        services.AddScoped<TestJob>();
        services.AddScoped<TestJob2>();

        //Đăng ký DelegatingHandler
        services.AddTransient<AuthorizationDelegatingHandler>();

        //đăng ký httpclient
        services.AddHttpClient<IdentityMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"http://{configuration["IdentityMicroserviceName"]}:{configuration["IdentityMicroservicePort"]}");
        }).AddHttpMessageHandler<AuthorizationDelegatingHandler>(); ;
        services.AddHttpClient<AcademyMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"http://{configuration["AcademyMicroserviceName"]}:{configuration["AcademyMicroservicePort"]}");
        }).AddHttpMessageHandler<AuthorizationDelegatingHandler>(); ;

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

