using Droniverse.Community.Application.DTO.Extensions;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.IService
{
    public interface IUserPrizeService
    {
        Task<PaginationResult<IEnumerable<UserPrizeResponse>>> GetUserPrizeByCurrentUser(GetUserPrizeCurrentUserSearchRequest request);
    }
}
