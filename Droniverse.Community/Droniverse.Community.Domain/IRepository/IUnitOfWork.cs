using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Domain.IRepository;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    IClubRepository Clubs { get; }
    IClubCategoryRepository ClubCategories { get; }
    IClubAttemptRequestRepository ClubAttemptRequests { get; }
    IClubCreationRequestRepository ClubCreationRequests { get; }
    ICompetitionRepository Competitions { get; }
    IMediaRepository Medias { get; }
    IMediaTypeRepository MediaTypes { get; }
    IParticipationRepository Participations { get; }
    IProductCategoryRepository ProductCategories { get; }
    IProductRepository Products { get; }
    IRoundRepository Rounds { get; }
    IClubCourseRepository ClubCourses { get; }

    Task<int> SaveChangeAsync();
}