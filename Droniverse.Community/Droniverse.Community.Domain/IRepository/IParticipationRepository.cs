using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Domain.IRepository;
public interface IParticipationRepository : IRepository<Participation>
{
    Task<int> CountMembersByClubIdAsync(Guid clubId);
    Task<bool> IsUserInClub(Guid clubId, Guid userId);
    Task<List<Guid>> GetActiveParticipantUserIdsByClubAsync(Guid clubId);
    Task<(int TotalRecords, IEnumerable<Participation> Participations)> GetActiveParticipationsByClubAsync(
        Guid clubId,
        int skip,
        int take,
        IEnumerable<Guid>? userIdsFilter = null);
}

