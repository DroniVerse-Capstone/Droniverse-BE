using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class UnitOfWork : IUnitOfWork
{
    private readonly MySqlDbContext _mySqlContext;
    private IRepository<Certificate> _certificate;
    private IRepository<Code> _code;
    private IRepository<CodeUsage> _codeUsage;
    private ICourseRepository _course;
    private IRepository<CourseVersion> _courseVersion;
    private IRepository<CourseVersionCategory> _courseVersionCategory;
    private IRepository<Drone> _drone;
    private IRepository<DroneType> _droneType;
    private IRepository<Enrollment> _enrollment;
    private IRepository<Feedback> _feedback;
    private IRepository<Lab> _lab;
    private IRepository<Lesson> _lesson;
    private IRepository<Module> _module;
    private IRepository<Quiz> _quiz;
    private IRepository<QuizAnswer> _quizAnswer;
    private IRepository<QuizQuestion> _quizQuestion;
    private IRepository<Report> _report;
    private IRepository<RequiredDrone> _requiredDrone;
    private IRepository<Theory> _theory;
    private IRepository<UserAttempt> _userAttempt;
    private IRepository<UserCertificate> _userCertificate;
    private IRepository<UserLab> _userLab;
    private IRepository<UserModule> _userModule;

    public UnitOfWork(MySqlDbContext mySqlContext)
    {
        _mySqlContext = mySqlContext;
    }
    public IRepository<Certificate> Certificates => _certificate ??= new CertificateRepository(_mySqlContext);

    public IRepository<Code> Codes => _code ??= new CodeRepository(_mySqlContext);

    public IRepository<CodeUsage> CodeUsages => _codeUsage ??= new CodeUsageRepository(_mySqlContext);

    public ICourseRepository Courses => _course ??= new CourseRepository(_mySqlContext);

    public IRepository<CourseVersion> CourseVersions => _courseVersion ??= new CourseVersionRepository(_mySqlContext);

    public IRepository<CourseVersionCategory> CourseVersionCategories => _courseVersionCategory ??= new CourseVersionCategoryRepository(_mySqlContext);

    public IRepository<Drone> Drones => _drone ??= new DroneRepository(_mySqlContext);

    public IRepository<DroneType> DroneTypes => _droneType ??= new DroneTypeRepository(_mySqlContext);

    public IRepository<Enrollment> Enrollments => _enrollment ??= new EnrollmentRepository(_mySqlContext);

    public IRepository<Feedback> Feedbacks => _feedback ??= new FeedbackRepository(_mySqlContext);

    public IRepository<Lab> Labs => _lab ??= new LabRepository(_mySqlContext);

    public IRepository<Lesson> Lessons => _lesson ??= new LessonRepository(_mySqlContext);

    public IRepository<Module> Modules => _module ??= new ModuleRepository(_mySqlContext);

    public IRepository<Quiz> Quizs => _quiz ??= new QuizRepository(_mySqlContext);

    public IRepository<QuizAnswer> QuizAnswers => _quizAnswer ??= new QuizAnswerRepository(_mySqlContext);
    public IRepository<QuizQuestion> QuizQuestions => _quizQuestion ??= new QuizQuestionRepository(_mySqlContext);

    public IRepository<Report> Reports => _report ??= new ReportRepository(_mySqlContext);

    public IRepository<RequiredDrone> RequiredDrones => _requiredDrone ??= new RequiredDroneRepository(_mySqlContext);

    public IRepository<Theory> Theories => _theory ??= new TheoryRepository(_mySqlContext);

    public IRepository<UserAttempt> UserAttempts => _userAttempt ??= new UserAttemptRepository(_mySqlContext);

    public IRepository<UserCertificate> UserCertificates => _userCertificate ??= new UserCertificateRepository(_mySqlContext);

    public IRepository<UserLab> UserLabs => _userLab ??= new UserLabRepository(_mySqlContext);

    public IRepository<UserModule> UserModules => _userModule ??= new UserModuleRepository(_mySqlContext);

    public void Dispose()
    {
        _mySqlContext.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _mySqlContext.SaveChangesAsync();
    }
}

