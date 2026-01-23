using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class UnitOfWork : IUnitOfWork
{
    private readonly MySqlDbContext _context;
    private IRepository<Category> _category;
    //private IRepository<Certificate> _certificate;
    private IRepository<Club> _club;
    private IRepository<ClubRequest> _clubRequest;
    //private IRepository<Code> _code;
    private IRepository<Competition> _competiton;
    //private IRepository<Course> _course;
    //private IRepository<Enrollment> _enrollment;
    //private IRepository<Feedback> _feedback;
    private IRepository<Media> _media;
    private IRepository<MediaType> _mediaType;
    //private IRepository<Module> _module;
    private IRepository<Participation> _participation;
    private IRepository<Product> _product;
    private IRepository<ProductCategory> _productCategory;
    public IRepository<Category> Categories => _category ??= new CategoryRepository(_context);

    //public IRepository<Certificate> Certificates => _certificate ??= new Repository<Certificate>(_context);

    public IRepository<Club> Clubs => _club ??= new ClubRepository(_context);

    public IRepository<ClubRequest> ClubRequests => _clubRequest ??= new ClubRequestRepository(_context);

    //public IRepository<Code> Codes => _code ??= new CodeRepository(_context);

    public IRepository<Competition> Competitions => _competiton ??= new CompetitionRepository(_context);

    //public IRepository<Course> Courses => _course ??= new CourseRepository(_context);

    //public IRepository<Enrollment> Enrollments => _enrollment ??= new EnrollmentRepository(_context);

    //public IRepository<Feedback> Feedbacks => _feedback ??= new FeedbackRepository(_context);
    public IRepository<Media> Medias => _media ??= new MediaRepository(_context);

    public IRepository<MediaType> MediaTypes => _mediaType ??= new MediaTypeRepository(_context);

    //public IRepository<Module> Modules => _module ??= new ModuleRepository(_context);

    public IRepository<Participation> Participations => _participation ??= new ParticipationRepository(_context);

    public IRepository<ProductCategory> ProductCategories => _productCategory ??= new ProductCategoryRepository(_context);

    public IRepository<Product> Products => _product ??= new ProductRepository(_context);

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

