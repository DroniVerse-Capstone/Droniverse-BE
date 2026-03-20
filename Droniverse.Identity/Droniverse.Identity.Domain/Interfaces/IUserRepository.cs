using Droniverse.Identity.Domain.Entities;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Domain.Interfaces;
public interface IUserRepository : IRepository<Account>
{
    Task<IEnumerable<UserResponse>> GetUsersByIdsAsync(IEnumerable<Guid> userIds);
}

