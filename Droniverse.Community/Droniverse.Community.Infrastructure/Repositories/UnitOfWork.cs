using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class UnitOfWork : IUnitOfWork
{
    private readonly MySqlDbContext _context;

    private IClubRepository _club;
    private IClubAttemptRequestRepository _clubAttemptRequest;
    private ICompetitionRepository _competition;
    private IMediaRepository _media;
    private IMediaTypeRepository _mediaType;
    private IParticipationRepository _participation;
    private IProductRepository _product;
    private IProductCategoryRepository _productCategory;
    private IRoundRepository _round;
    private IClubCreationRequestRepository _clubCreationRequest;
    private ICompetitionPrizeRepository _competitionPrize;
    private IUserPrizeRepository _userPrize;
    private IUserCompetitionRepository _userCompetition;
    private IUserRoundRepository _userRound;
    private IWalletRepository _wallet;
    public UnitOfWork(MySqlDbContext context)
    {
        _context = context;
    }

    public IClubRepository Clubs
        => _club ??= new ClubRepository(_context);

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

    public IClubCreationRequestRepository ClubCreationRequests => _clubCreationRequest ??= new ClubCreationRequestRepository(_context);

    public IClubAttemptRequestRepository ClubAttemptRequests => _clubAttemptRequest ??= new ClubAttemptRequestRepository(_context);

    public ICompetitionPrizeRepository CompetitionPrizes => _competitionPrize ??= new CompetitionPrizeRepository(_context);

    public IUserPrizeRepository UserPrizes => _userPrize ??= new UserPrizeRepository(_context);
    public IUserCompetitionRepository UserCompetitions => _userCompetition ??= new UserCompetitionRepository(_context);
    public IUserRoundRepository UserRounds => _userRound ??= new UserRoundRepository(_context);

    public IWalletRepository Wallets => _wallet ??= new WalletRepository(_context);

    public async Task<int> SaveChangeAsync()
    {
        //var entries = _context.ChangeTracker.Entries();

        //foreach (var e in entries)
        //{
        //    Console.WriteLine($"{e.Entity.GetType().Name} - {e.State}");
        //}

        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

