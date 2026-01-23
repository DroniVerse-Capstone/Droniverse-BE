using Droniverse.Community.Domain.Entities;
using System.Data;
using System.Security;

namespace Droniverse.Community.Domain.IRepository;
public interface IUnitOfWork : IDisposable
{
    IRepository<Category> Categories { get; }
    //IRepository<Certificate> Certificates { get; }
    IRepository<Club> Clubs { get; }
    IRepository<ClubRequest> ClubRequests { get; }
    //IRepository<Code> Codes { get; }
    IRepository<Competition> Competitions { get; }
    //IRepository<Course> Courses { get; }
    //IRepository<Enrollment> Enrollments { get; }
    //IRepository<Feedback> Feedbacks { get; }
    IRepository<Media> Medias { get; }
    IRepository<MediaType> MediaTypes { get; }
    IRepository<Participation> Participations { get; }
    IRepository<ProductCategory> ProductCategories { get; }
    IRepository<Product> Products { get; }

    Task<int> SaveChangeAsync();
}

