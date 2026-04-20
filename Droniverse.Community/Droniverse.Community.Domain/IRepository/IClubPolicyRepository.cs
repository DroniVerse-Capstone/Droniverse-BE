using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Domain.IRepository;
public interface IClubPolicyRepository : IRepository<ClubPolicy>
{
    Task<PaginationResult<IEnumerable<ClubPolicy>>> GetAll(
        int currentPage = 1,
        int pageSize = 5);
    
}

