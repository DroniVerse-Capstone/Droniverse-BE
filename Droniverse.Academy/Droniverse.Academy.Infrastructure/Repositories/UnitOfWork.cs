using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class UnitOfWork : IUnitOfWork
{
    private readonly MySqlDbContext _mySqlContext;
    private ICertificateRepository _certificate;
    private ICodeRepository _code;
    private ICourseRepository _course;
    private ICourseVersionRepository _courseVersion;
    private IDroneRepository _drone;
    private IDroneTypeRepository _droneType;
    private IEnrollmentRepository _enrollment;
    private IFeedbackRepository _feedback;
    private IFlightSimulatorRepository _flightSimulator;
    private ILabRepository _lab;
    private ILessonRepository _lesson;
    private IModuleRepository _module;
    private IQuizRepository _quiz;
    private IQuizAttemptRepository _quizAttempt;
    private IQuizQuestionAttemptRepository _quizQuestionAttempt;
    private IQuizQuestionRepository _quizQuestion;
    private IReportRepository _report;
    private IStructureSimulatorRepository _structureSimulator;
    private ITheoryRepository _theory;
    private IUserCertificateRepository _userCertificate;
    private IUserLabRepository _userLab;
    private IUserLessonRepository _userLesson;
    private IUserModuleRepository _userModule;
    private ILevelRepository _level;
    private IPrerequisiteCourseRepository _prerequisiteCourse;

    public UnitOfWork(MySqlDbContext mySqlContext)
    {
        _mySqlContext = mySqlContext;
    }

    public ICertificateRepository Certificates => _certificate ??= new CertificateRepository(_mySqlContext);

    public ICodeRepository Codes => _code ??= new CodeRepository(_mySqlContext);

    public ICourseRepository Courses => _course ??= new CourseRepository(_mySqlContext);

    public ICourseVersionRepository CourseVersions => _courseVersion ??= new CourseVersionRepository(_mySqlContext);

    public IDroneRepository Drones => _drone ??= new DroneRepository(_mySqlContext);

    public IDroneTypeRepository DroneTypes => _droneType ??= new DroneTypeRepository(_mySqlContext);

    public IEnrollmentRepository Enrollments => _enrollment ??= new EnrollmentRepository(_mySqlContext);

    public IFeedbackRepository Feedbacks => _feedback ??= new FeedbackRepository(_mySqlContext);

    public IFlightSimulatorRepository FlightSimulators => _flightSimulator ??= new FlightSimulatorRepository(_mySqlContext);

    public ILabRepository Labs => _lab ??= new LabRepository(_mySqlContext);

    public ILessonRepository Lessons => _lesson ??= new LessonRepository(_mySqlContext);

    public IModuleRepository Modules => _module ??= new ModuleRepository(_mySqlContext);

    public IQuizRepository Quizs => _quiz ??= new QuizRepository(_mySqlContext);

    public IQuizAttemptRepository QuizAttempts => _quizAttempt ??= new QuizAttemptRepository(_mySqlContext);

    public IQuizQuestionAttemptRepository QuizQuestionAttempts => _quizQuestionAttempt ??= new QuizQuestionAttemptRepository(_mySqlContext);

    public IQuizQuestionRepository QuizQuestions => _quizQuestion ??= new QuizQuestionRepository(_mySqlContext);

    public IReportRepository Reports => _report ??= new ReportRepository(_mySqlContext);

    public IStructureSimulatorRepository StructureSimulators => _structureSimulator ??= new StructureSimulatorRepository(_mySqlContext);

    public ITheoryRepository Theories => _theory ??= new TheoryRepository(_mySqlContext);

    public IUserCertificateRepository UserCertificates => _userCertificate ??= new UserCertificateRepository(_mySqlContext);

    public IUserLabRepository UserLabs => _userLab ??= new UserLabRepository(_mySqlContext);

    public IUserLessonRepository UserLessons => _userLesson ??= new UserLessonRepository(_mySqlContext);

    public IUserModuleRepository UserModules => _userModule ??= new UserModuleRepository(_mySqlContext);

    public ILevelRepository Levels => _level ??= new LevelRepository(_mySqlContext);

    public IPrerequisiteCourseRepository PrerequisiteCourses => _prerequisiteCourse ??= new PrerequisiteCourseRepository(_mySqlContext);

    public void Dispose()
    {
        _mySqlContext.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _mySqlContext.SaveChangesAsync();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await using var transaction = await _mySqlContext.Database.BeginTransactionAsync();
        try
        {
            await action();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
    {
        await using var transaction = await _mySqlContext.Database.BeginTransactionAsync();
        try
        {
            var result = await action();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

