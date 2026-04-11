using Droniverse.Community.Application.Delegate;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Application.Mapper;
using Droniverse.Community.Application.Services;
using Droniverse.Community.Application.Services.Mongo;
using Droniverse.Community.Application.States.CompetitionState;
using Droniverse.Community.Application.States.RoundState;
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
        services.AddScoped<INotificationService, EmailNotificationService>();
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
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IClubCourseService, ClubCourseService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<ICompetitionState, DraftState>();
        services.AddScoped<ICompetitionState, PublishedState>();
        services.AddScoped<ICompetitionState, ResultPublishedState>();
        services.AddScoped<ICompetitionState, CancelledState>();
        services.AddScoped<ICompetitionState, InvalidState>();

        services.AddScoped<ICompetitionStateFactory, CompetitionStateFactory>();
        services.AddScoped<CompetitionLifecycleService>();

        services.AddScoped<IRoundState, PendingRoundState>();
        services.AddScoped<IRoundState, ScheduleInvalidRoundState>();

        services.AddScoped<IRoundStateFactory, RoundStateFactory>();
        services.AddScoped<RoundLifecycleService>();
        //services.AddScoped<IPaymentService, PaymentService>();

        //Đăng ký DelegatingHandler
        services.AddTransient<AuthorizationDelegatingHandler>();

        //đăng ký httpclient
        services.AddHttpClient<IdentityMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"https://{configuration["IdentityMicroserviceName"]}:{configuration["IdentityMicroservicePort"]}");
        }).AddHttpMessageHandler<AuthorizationDelegatingHandler>();
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

