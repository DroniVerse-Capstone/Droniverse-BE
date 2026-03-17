using Droniverse.Academy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql;

public class MySqlDbContext : DbContext
{
    public MySqlDbContext(DbContextOptions<MySqlDbContext> options) : base(options)
    {
    }
    public DbSet<Code> Codes { get; set; }
    public DbSet<CodeUsage> CodeUsages { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseVersion> CourseVersions { get; set; }
    public DbSet<CourseVersionCategory> CourseVersionCategories { get; set; }
    public DbSet<Drone> Drones { get; set; }
    public DbSet<DroneType> DroneTypes { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<Lab> Labs { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<QuizQuestionAttempt> QuizQuestionAttempts { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<RequiredDrone> RequiredDrones { get; set; }
    public DbSet<Theory> Theories { get; set; }
    public DbSet<UserLab> UserLabs { get; set; }
    public DbSet<UserLesson> UserLessons { get; set; }
    public DbSet<UserModule> UserModules { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Apply all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MySqlDbContext).Assembly,
            type => type.Namespace != null &&
                    type.Namespace.Contains("MySql.Configurations"));
    }
}
