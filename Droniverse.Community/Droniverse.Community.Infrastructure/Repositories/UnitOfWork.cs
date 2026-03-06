using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class UnitOfWork : IUnitOfWork
{
    private readonly MySqlDbContext _context;

    private ICategoryRepository _category;
    private IClubRepository _club;
    private IClubCategoryRepository _clubCategory;
    private IClubAttemptRequestRepository _clubAttemptRequest;
    private ICompetitionRepository _competition;
    private IMediaRepository _media;
    private IMediaTypeRepository _mediaType;
    private IParticipationRepository _participation;
    private IProductRepository _product;
    private IProductCategoryRepository _productCategory;
    private IRoundRepository _round;
    private IClubCourseRepository _clubCourse;
    private IClubCreationRequestRepository _clubCreationRequest;
    private ICompetitionPrizeRepository _competitionPrize;
    private IUserPrizeRepository _userPrize;

    public UnitOfWork(MySqlDbContext context)
    {
        _context = context;
    }

    public ICategoryRepository Categories
        => _category ??= new CategoryRepository(_context);

    public IClubRepository Clubs
        => _club ??= new ClubRepository(_context);

    public IClubCategoryRepository ClubCategories
        => _clubCategory ??= new ClubCategoryRepository(_context);

    public ICompetitionRepository Competitions
        => _competition ??= new CompetitionRepository(_context);

    public IMediaRepository Medias
        => _media ??= new MediaRepository(_context);

    public IMediaTypeRepository MediaTypes
        => _mediaType ??= new MediaTypeRepository(_context);

    public IParticipationRepository Participations
        => _participation ??= new ParticipationRepository(_context);

    public IProductCategoryRepository ProductCategories
        => _productCategory ??= new ProductCategoryRepository(_context);

    public IProductRepository Products
        => _product ??= new ProductRepository(_context);

    public IRoundRepository Rounds
        => _round ??= new RoundRepository(_context);

    public IClubCourseRepository ClubCourses => _clubCourse ??= new ClubCourseRepository(_context);

    public IClubCreationRequestRepository ClubCreationRequests => _clubCreationRequest ??= new ClubCreationRequestRepository(_context);

    public IClubAttemptRequestRepository ClubAttemptRequests => _clubAttemptRequest ??= new ClubAttemptRequestRepository(_context);

    public ICompetitionPrizeRepository CompetitionPrizes => _competitionPrize ??= new CompetitionPrizeRepository(_context);

    public IUserPrizeRepository UserPrizes => _userPrize ??= new UserPrizeRepository(_context);

    public async Task<int> SaveChangeAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

