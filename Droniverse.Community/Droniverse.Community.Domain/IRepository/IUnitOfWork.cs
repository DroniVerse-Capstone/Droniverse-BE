using Droniverse.Community.Domain.Entities;
using System.Data;
using System.Security;

namespace Droniverse.Community.Domain.IRepository;
public interface IUnitOfWork : IDisposable
{
    IRepository<Category> Categories { get; }
    IRepository<Club> Clubs { get; }
    IRepository<ClubRequest> ClubRequests { get; }
    IRepository<Competition> Competitions { get; }
    IRepository<Media> Medias { get; }
    IRepository<MediaType> MediaTypes { get; }
    IRepository<Participation> Participations { get; }
    IRepository<ProductCategory> ProductCategories { get; }
    IRepository<Product> Products { get; }
    IRepository<Round> Rounds { get; }
    Task<int> SaveChangeAsync();
}

