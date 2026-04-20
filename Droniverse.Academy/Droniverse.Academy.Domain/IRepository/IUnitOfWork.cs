using Droniverse.Academy.Domain.Entities;
namespace Droniverse.Academy.Domain.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        ICertificateRepository Certificates { get; }
        ICodeRepository Codes { get; }
        ICourseRepository Courses { get; }
        ICourseVersionRepository CourseVersions { get; }
        IDroneRepository Drones { get; }
        IDroneTypeRepository DroneTypes { get; }
        IEnrollmentRepository Enrollments { get; }
        IFeedbackRepository Feedbacks { get; }
        IFlightSimulatorRepository FlightSimulators { get; }
        ILabRepository Labs { get; }
        ILessonRepository Lessons { get; }
        IModuleRepository Modules { get; }
        IQuizRepository Quizs { get; }
        IQuizAttemptRepository QuizAttempts { get; }
        IQuizQuestionAttemptRepository QuizQuestionAttempts { get; }
        IQuizQuestionRepository QuizQuestions { get; }
        IReportRepository Reports { get; }
        IStructureSimulatorRepository StructureSimulators { get; }
        ITheoryRepository Theories { get; }
        IUserCertificateRepository UserCertificates { get; }
        IUserLabRepository UserLabs { get; }
        IUserLessonRepository UserLessons { get; }
        IUserModuleRepository UserModules { get; }
        ILevelRepository Levels { get; }

        Task ExecuteInTransactionAsync(Func<Task> action);
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
        Task<int> SaveChangesAsync();
    }
}