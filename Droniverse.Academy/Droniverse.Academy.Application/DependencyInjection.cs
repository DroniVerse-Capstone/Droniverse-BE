using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.IService.Mongo;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Delegate;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.Mapper;
using Droniverse.Academy.Application.Services;
using Droniverse.Academy.Application.Services.Duplication;
using Droniverse.Academy.Application.Services.Mongo;
using Droniverse.Shared.Services;
using Droniverse.Shared.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Droniverse.Academy.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(CourseMappingProfile).Assembly); //chỉ cần thêm 1 profile là đc
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ICourseVersionService, CourseVersionService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<ILabService, LabService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IQuizQuestionService, QuizQuestionService>();
        services.AddScoped<ITheoryService, TheoryService>();
        services.AddScoped<IWebSimulatorService, WebSimulatorService>();
        services.AddScoped<IVRSimulatorService, VRSimulatorService>();
        services.AddScoped<IDroneTypeService, DroneTypeService>();
        services.AddScoped<IDroneService, DroneService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<ICertificateCreationService, CertificateCreationService>();
        services.AddScoped<ICertificateImageService, CertificateImageService>();
        services.AddScoped<IUserCertificateService, UserCertificateService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<LearningContextLoader>();
        services.AddScoped<LearningProgressService>();
        services.AddScoped<LearningPathAssembler>();
        services.AddScoped<LearningCertificateService>();
        services.AddScoped<LearningAssessmentAccessService>();
        services.AddScoped<ILearningService, LearningService>();
        services.AddScoped<IQuizLearningService, QuizLearningService>();
        services.AddScoped<ILabLearningService, LabLearningService>();
        services.AddScoped<IUserModuleService, UserModuleService>();
        services.AddScoped<IUserLessonService, UserLessonService>();
        services.AddScoped<IUserLabService, UserLabService>();
        services.AddScoped<IUserLevelService, UserLevelService>();
        services.AddScoped<IUserQuizAttemptService, UserQuizAttemptService>();
        services.AddScoped<IUserQuizQuestionAttemptService, UserQuizQuestionAttemptService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAdminEnrollmentService, AdminEnrollmentService>();
        services.AddScoped<IAdminReportService, AdminReportService>();
        services.AddScoped<IAdminUserCertificateService, AdminUserCertificateService>();
        services.AddScoped<IAdminUserLearningService, AdminUserLearningService>();
        services.AddScoped<ILabContentService, LabContentService>();
        services.AddScoped<IUserDisplayNameService, UserDisplayNameService>();
        services.AddScoped<IUserLookupService, UserLookupService>();

        services.AddScoped<ICourseVersionDuplicator, CourseVersionDuplicator>();
        services.AddScoped<IModuleDuplicator, ModuleDuplicator>();
        services.AddScoped<ILessonDuplicator, LessonDuplicator>();
        services.AddScoped<ILessonReferenceDuplicator, LessonReferenceDuplicator>();
        services.AddScoped<ITheoryDuplicator, TheoryDuplicator>();
        services.AddScoped<IQuizDuplicator, QuizDuplicator>();
        services.AddScoped<ILabDuplicator, LabDuplicator>();
        services.AddScoped<IWebSimulatorDuplicator, WebSimulatorDuplicator>();
        services.AddScoped<IVRSimulatorDuplicator, VRSimulatorDuplicator>();
        services.AddScoped<ILabContentSyncService, LabContentSyncService>();

        services.AddScoped<ICodeService, CodeService>();
        services.AddScoped<IImportService, ImportService>();
        services.AddScoped<ICacheService, CacheService>();

        services.AddScoped<ILevelService, LevelService>();
        services.AddScoped<IPrerequisiteCourseService, PrerequisiteCourseService>();

        services.AddTransient<AuthorizationDelegatingHandler>();

        //đăng ký httpclient
        services.AddHttpClient<IdentityMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"http://{configuration["IdentityMicroserviceName"]}:{configuration["IdentityMicroservicePort"]}");
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

        services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }

}

