using Droniverse.Academy.Domain.Entities;
namespace Droniverse.Academy.Domain.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Certificate> Certificates { get; }
        IRepository<Code> Codes { get; }
        IRepository<CodeUsage> CodeUsages { get; }
        IRepository<Course> Courses { get; }
        IRepository<CourseVersion> CourseVersions { get; }
        IRepository<CourseVersionCategory> CourseVersionCategories { get; }
        IRepository<Drone> Drones { get; }
        IRepository<DroneType> DroneTypes { get; }
        IRepository<Enrollment> Enrollments { get; }
        IRepository<Feedback> Feedbacks { get; }
        IRepository<Lab> Labs { get; }
        IRepository<Lesson> Lessons { get; }
        IRepository<Module> Modules { get; }
        IRepository<Quiz> Quizs { get; }
        IRepository<QuizAnswer> QuizAnswers { get; }
        IRepository<QuizQuestion> QuizQuestions { get; }
        IRepository<Report> Reports { get; }
        IRepository<RequiredDrone> RequiredDrones { get; }
        IRepository<Theory> Theories { get; }
        IRepository<UserAttempt> UserAttempts { get; }
        IRepository<UserCertificate> UserCertificates { get; }
        IRepository<UserLab> UserLabs { get; }
        IRepository<UserModule> UserModules { get; }

        Task<int> SaveChangesAsync();
    }
}