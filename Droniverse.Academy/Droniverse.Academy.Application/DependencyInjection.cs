using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.IService.Mongo;
using Droniverse.Academy.Application.IService.Duplication;
﻿using Droniverse.Academy.Application.Delegate;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.Mapper;
using Droniverse.Academy.Application.Services;
using Droniverse.Academy.Application.Services.Duplication;
using Droniverse.Academy.Application.Services.Mongo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Droniverse.Academy.Application;
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(typeof(CourseMappingProfile).Assembly); //chỉ cần thêm 1 profile là đc
        // ensure new mapping profiles are picked up
        services.AddAutoMapper(typeof(CategoryMappingProfile).Assembly);
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ICourseVersionService, CourseVersionService>();
        services.AddScoped<ICourseVersionCategoryService, CourseVersionCategoryService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<ILabService, LabService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IQuizQuestionService, QuizQuestionService>();
        services.AddScoped<ITheoryService, TheoryService>();
        services.AddScoped<IDroneTypeService, DroneTypeService>();
        services.AddScoped<IDroneService, DroneService>();
        services.AddScoped<IRequiredDroneService, RequiredDroneService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<IUserCertificateService, UserCertificateService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<ILabContentService, LabContentService>();
        services.AddScoped<IUserDisplayNameService, UserDisplayNameService>();

        services.AddScoped<ICourseVersionDuplicator, CourseVersionDuplicator>();
        services.AddScoped<IModuleDuplicator, ModuleDuplicator>();
        services.AddScoped<ILessonDuplicator, LessonDuplicator>();
        services.AddScoped<ILessonReferenceDuplicator, LessonReferenceDuplicator>();
        services.AddScoped<ITheoryDuplicator, TheoryDuplicator>();
        services.AddScoped<IQuizDuplicator, QuizDuplicator>();
        services.AddScoped<ILabDuplicator, LabDuplicator>();
        services.AddScoped<ILabContentSyncService, LabContentSyncService>();

        services.AddTransient<AuthorizationDelegatingHandler>();

        //đăng ký httpclient
        services.AddHttpClient<IdentityMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"http://{configuration["IdentityMicroserviceName"]}:{configuration["IdentityMicroservicePort"]}");
        }).AddHttpMessageHandler<AuthorizationDelegatingHandler>(); ;
        services.AddHttpClient<CommunityMicroserviceClient>(client =>
        {
            client.BaseAddress = new Uri($"http://{configuration["CommunityMicroserviceName"]}:{configuration["CommunityMicroservicePort"]}");
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

