using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Infrastructure.Persistence.MySql.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Persistence.MySql;
public class MySqlDbContext : DbContext
{
    public MySqlDbContext(DbContextOptions<MySqlDbContext> options) : base(options)
    {
    }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Club> Clubs { get; set; }
    public DbSet<ClubCategory> ClubCategories { get; set; }
    public DbSet<ClubCourse> ClubCourses { get; set; }
    public DbSet<ClubAttemptRequest> ClubRequests { get; set; }
    public DbSet<Competition> Competitions { get; set; }
    public DbSet<CompetitionCertificate> CompetitionCertificates { get; set; }
    public DbSet<Media> Medias { get; set; }
    public DbSet<MediaType> MediaTypes { get; set; }
    public DbSet<Participation> Participations { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<UserProduct> UserProducts { get; set; }
    public DbSet<Round> Rounds { get; set; }
    public DbSet<UserRound> UserRounds { get; set; }
    public DbSet<UserCompetition> UserCompetitions { get; set; }
    public DbSet<ClubCreationRequest> ClubCreationRequests { get; set; }
    public DbSet<ClubCreationRequestCategory> ClubCreationRequestCategories { get; set; }
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

