using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class UnitOfWork : IUnitOfWork
{
    private readonly MySqlDbContext _context;

    private IRepository<Category> _category;
    private IRepository<Club> _club;
    private IRepository<ClubRequest> _clubRequest;
    private IRepository<Competition> _competiton;
    private IRepository<Media> _media;
    private IRepository<MediaType> _mediaType;
    private IRepository<Participation> _participation;
    private IRepository<Product> _product;
    private IRepository<ProductCategory> _productCategory;
    private IRepository<Round> _round;
    public UnitOfWork(MySqlDbContext context)
    {
        _context = context;
    }
    public IRepository<Category> Categories => _category ??= new CategoryRepository(_context);

    public IRepository<Club> Clubs => _club ??= new ClubRepository(_context);

    public IRepository<ClubRequest> ClubRequests => _clubRequest ??= new ClubRequestRepository(_context);

    public IRepository<Competition> Competitions => _competiton ??= new CompetitionRepository(_context);
    public IRepository<Media> Medias => _media ??= new MediaRepository(_context);

    public IRepository<MediaType> MediaTypes => _mediaType ??= new MediaTypeRepository(_context);

    public IRepository<Participation> Participations => _participation ??= new ParticipationRepository(_context);

    public IRepository<ProductCategory> ProductCategories => _productCategory ??= new ProductCategoryRepository(_context);

    public IRepository<Product> Products => _product ??= new ProductRepository(_context);

    public IRepository<Round> Rounds => _round ??= new RoundRepository(_context);

    public void Dispose() // dùng để đóng kết nối với DbContext
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<int> SaveChangeAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

