using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Domain.IRepository;
public interface IClubAttemptRequestRepository : IRepository<ClubAttemptRequest>
{
    Task<bool> IsUserInClubAttemptRequest(Guid userID, Guid clubID);

}

