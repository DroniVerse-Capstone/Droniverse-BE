using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.Enums.SeachRequest;
using MongoDB.Driver;

namespace Droniverse.Community.Domain.IRepository;

public interface IClubAttemptRequestRepository : IRepository<ClubAttemptRequest>
{
    Task<bool> IsUserInClubAttemptRequest(Guid userID, Guid clubID);

    /// <summary>
    /// Get paginated and filtered ClubAttemptRequests with optimized query
    /// </summary>
    Task<(IEnumerable<ClubAttemptRequest> Items, int TotalCount)> GetFilteredRequestsAsync(
     Guid clubID,
     ClubAttemptRequestStatus? status,
     DateTime? createdFrom,
     DateTime? createdTo,
     DateTime? processedFrom,
     DateTime? processedTo,
     ClubAttemptRequestSortBy sortBy,
     Enums.SeachRequest.SortDirection sortDirection,
     int skip,
     int take);
}

