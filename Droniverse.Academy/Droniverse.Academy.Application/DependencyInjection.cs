using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.Mapper;
using Droniverse.Academy.Application.Services;
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
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IQuizQuestionService, QuizQuestionService>();
        services.AddScoped<ITheoryService, TheoryService>();
        services.AddScoped<IDroneTypeService, DroneTypeService>();
        services.AddScoped<IDroneService, DroneService>();
        services.AddScoped<IRequiredDroneService, RequiredDroneService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<IUserCertificateService, UserCertificateService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        return services;
    }

}

