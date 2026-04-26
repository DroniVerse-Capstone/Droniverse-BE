using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.QueryModels;

namespace Droniverse.Community.Domain.IRepository
{
    public interface IUserPrizeRepository : IRepository<UserPrize>
    {
        Task AddRange(IEnumerable<UserPrize> entities);
        Task<(IEnumerable<UserPrizeQueryModel> Items, int TotalRecords)> GetUserPrizeCurrentByUserId(Guid userId, string? competitionName, int page, int pageSize);

    }
}
