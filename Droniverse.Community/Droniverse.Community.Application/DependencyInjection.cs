using Droniverse.Community.Application.Delegate;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Application.Mapper;
using Droniverse.Community.Application.RabbitMQ;
using Droniverse.Community.Application.Services;
using Droniverse.Community.Application.Services.Mongo;
using Droniverse.Community.Application.States.CompetitionState;
using Droniverse.Community.Application.States.RoundState;
using Droniverse.Shared.Messages.Notification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Droniverse.Community.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Chỉ cần thêm 1 mapping profile là đc
        services.AddAutoMapper(typeof(ClubMappingProfile).Assembly);
        services.AddAutoMapper(typeof(ClubRequestMappingProfile).Assembly); services.AddAutoMapper(typeof(TransactionMappingProfile).Assembly); services.AddScoped<INotificationService, EmailNotificationService>();
        services.AddScoped<IClubService, ClubService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IClubAttemptRequestService, ClubAttemptRequestService>();
        services.AddScoped<IClubCreationRequestService, ClubCreationRequestService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICompetitionService, CompetitionService>();
        services.AddScoped<ICompetitionCertificateService, CompetitionCertificateService>();
        services.AddScoped<IRoundService, RoundService>();
        services.AddScoped<ICompetitionPrizeService, CompetitionPrizeService>();
        services.AddScoped<IUserRoundService, UserRoundService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<ICompetitionLevelService, CompetitionLevelService>();
        services.AddScoped<ICompetitionState, DraftState>();

        // Đăng ký RabbitMQ Publisher cho notification
        services.AddScoped<IOrderNotificationPublisher, OrderNotificationPublisher>();

        // Đăng ký RabbitMQ Consumer cho user updates
        services.AddSingleton<UserNameUpdateConsumer>();
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
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<ITransactionService, TransactionService>();

        //Đăng ký DelegatingHandler
        services.AddTransient<AuthorizationDelegatingHandler>();

        //đăng ký httpclient
        services.AddHttpClient<IdentityMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"http://{configuration["IdentityMicroserviceName"]}:{configuration["IdentityMicroservicePort"]}");
            client.Timeout = TimeSpan.FromSeconds(30); // Set timeout to 30 seconds
        }).AddHttpMessageHandler<AuthorizationDelegatingHandler>();
        services.AddHttpClient<AcademyMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"http://{configuration["AcademyMicroserviceName"]}:{configuration["AcademyMicroservicePort"]}");
            client.Timeout = TimeSpan.FromSeconds(30); // Set timeout to 30 seconds
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

