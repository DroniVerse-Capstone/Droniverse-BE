using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Domain.IRepository;
public interface IClubRepository : IRepository<Club>
{
    Task<IEnumerable<Club>> GetAllWithCategories();
    Task<Club?> GetByIdWithCategories(Guid clubId);
    Task<IEnumerable<Club>> GetClubsByActiveParticipantUserId(Guid userId);
    Task<Dictionary<Guid, int>> GetMemberCountsByClubIds(IEnumerable<Guid> clubIds);
    Task<Dictionary<Guid, int>> GetCourseCountsByClubIds(IEnumerable<Guid> clubIds);
}

