using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.IRepository;
public interface IClubRepository : IRepository<Club>
{
    Task<IEnumerable<Club>> GetAllWithCategories();
    Task<Club?> GetByIdWithCategories(Guid clubId);
    Task<Club?> GetByClubCodeWithCategories(string clubCode);
    Task<IEnumerable<Club>> GetClubsByClubManagerID(Guid clubManagerID, ClubStatus? status = null);
    Task<IEnumerable<Club>> GetClubsByParticipantUserId(Guid userId, ClubStatus? status = null);
    Task<Dictionary<Guid, int>> GetMemberCountsByClubIds(IEnumerable<Guid> clubIds);
    Task<Dictionary<Guid, int>> GetCourseCountsByClubIds(IEnumerable<Guid> clubIds);
}

