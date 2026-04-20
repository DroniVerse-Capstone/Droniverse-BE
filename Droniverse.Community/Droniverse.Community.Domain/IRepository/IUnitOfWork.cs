using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Domain.IRepository;

public interface IUnitOfWork : IDisposable
{
    IClubRepository Clubs { get; }
    IClubAttemptRequestRepository ClubAttemptRequests { get; }
    IClubCreationRequestRepository ClubCreationRequests { get; }
    ICompetitionRepository Competitions { get; }
    IMediaRepository Medias { get; }
    IMediaTypeRepository MediaTypes { get; }
    IParticipationRepository Participations { get; }
    IProductCategoryRepository ProductCategories { get; }
    IProductRepository Products { get; }
    IRoundRepository Rounds { get; }
    ICompetitionPrizeRepository CompetitionPrizes { get; }
    IUserPrizeRepository UserPrizes { get; }
    IUserCompetitionRepository UserCompetitions { get; }
    IUserRoundRepository UserRounds { get; }


    Task<int> SaveChangeAsync();
}