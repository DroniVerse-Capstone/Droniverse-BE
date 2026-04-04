using Droniverse.Identity.Domain.Entities;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;

namespace Droniverse.Identity.Domain.Interfaces;
public interface IUserRepository : IRepository<Account>
{
    Task<IEnumerable<UserResponse>> GetUsersByIdsAsync(IEnumerable<Guid> userIds);
    Task<IEnumerable<SimpleUserReponse>> GetUsersByUserInfoAsync(
        string? searchName,
        SortDirection sortDirection,
        int currentPage,
        int pageSize);
}

