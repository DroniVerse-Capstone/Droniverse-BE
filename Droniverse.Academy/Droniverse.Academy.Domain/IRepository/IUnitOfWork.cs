using Droniverse.Academy.Domain.Entities;
namespace Droniverse.Academy.Domain.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        ICertificateRepository Certificates { get; }
        ICodeRepository Codes { get; }
        ICodeUsageRepository CodeUsages { get; }
        ICourseRepository Courses { get; }
        ICourseVersionRepository CourseVersions { get; }
        ICourseVersionCategoryRepository CourseVersionCategories { get; }
        IDroneRepository Drones { get; }
        IDroneTypeRepository DroneTypes { get; }
        IEnrollmentRepository Enrollments { get; }
        IFeedbackRepository Feedbacks { get; }
        ILabRepository Labs { get; }
        ILessonRepository Lessons { get; }
        IModuleRepository Modules { get; }
        IQuizRepository Quizs { get; }
        IQuizQuestionRepository QuizQuestions { get; }
        IReportRepository Reports { get; }
        IRequiredDroneRepository RequiredDrones { get; }
        ITheoryRepository Theories { get; }
        IUserCertificateRepository UserCertificates { get; }
        IUserLabRepository UserLabs { get; }
        IUserModuleRepository UserModules { get; }

        Task<int> SaveChangesAsync();
    }
}