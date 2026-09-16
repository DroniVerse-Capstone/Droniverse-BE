using Droniverse.Community.Domain.Entities;
using Droniverse.Shared.Enums;

namespace Droniverse.Community.Domain.IRepository;
public interface IParticipationRepository : IRepository<Participation>
{
    Task<Club> GetClubByApproverId(Guid userId);
    Task<int> CountMembersByClubIdAsync(Guid clubId);
    Task<bool> IsUserInClub(Guid clubId, Guid userId);
    Task<List<Guid>> GetActiveParticipantUserIdsByClubAsync(Guid clubId);
    Task<(int TotalRecords, IEnumerable<Participation> Participations)> GetActiveParticipationsByClubAsync(
        Guid clubId,
        int skip,
        int take,
        IEnumerable<Guid>? userIdsFilter = null);

    Task<List<Guid>> GetParicipantIdsByClubId(Guid clubId, ParticipationStatus participationStatus);
}

