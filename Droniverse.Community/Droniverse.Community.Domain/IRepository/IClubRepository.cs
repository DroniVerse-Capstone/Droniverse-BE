using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Domain.IRepository;
public interface IClubRepository : IRepository<Club>
{
    Task<PaginationResult<IEnumerable<Club>>> GetAllWithCategories(
        string? clubName = null,
        ClubStatus? clubStatus = null,
        int currentPage = 1,
        int pageSize = 5);

    Task<Club?> GetByIdWithCategories(Guid clubId);
    Task<Club?> GetByClubCodeWithCategories(string clubCode);
    Task<IEnumerable<Club>> GetClubsByClubManagerID(Guid clubManagerID, ClubStatus? status = null);
    Task<IEnumerable<Club>> GetClubsByParticipantUserId(Guid userId, ClubStatus? status = null);
    Task<Dictionary<Guid, (int MemberCount, int CourseCount)>> GetClubStatsByClubIds(IEnumerable<Guid> clubIds);
    Task<Dictionary<Guid, int>> GetMemberCountsByClubIds(IEnumerable<Guid> clubIds);
    Task<Dictionary<Guid, int>> GetCourseCountsByClubIds(IEnumerable<Guid> clubIds);
    Task<SimpleClubResponse?> GetSimpleClubInfoById(Guid clubId);
    
}

