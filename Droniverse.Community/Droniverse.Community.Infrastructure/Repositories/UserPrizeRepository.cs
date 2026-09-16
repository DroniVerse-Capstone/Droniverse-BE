using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Domain.QueryModels;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Repositories
{
    public class UserPrizeRepository : MySqlRepository<UserPrize>, IUserPrizeRepository
    {
        public UserPrizeRepository(MySqlDbContext context) : base(context)
        {
        }

        public async Task AddRange(IEnumerable<UserPrize> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public async Task<(IEnumerable<UserPrizeQueryModel> Items, int TotalRecords)>
   GetUserPrizeCurrentByUserId(
       Guid userId,
       string? competitionName,
       int page,
       int pageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.UserID == userId);

            if (!string.IsNullOrWhiteSpace(competitionName))
            {
                var keyword = competitionName.Trim();

                query = query.Where(x =>
                    x.Competition.NameVN.Contains(keyword) ||
                    x.Competition.NameEN.Contains(keyword));
            }

            var projected = query.Select(x => new UserPrizeQueryModel
            {
                CompetitionID = x.CompetitionID,
                NameVN = x.Competition.NameVN,
                NameEN = x.Competition.NameEN,

                PrizeId = x.PrizeID,
                TitleVN = x.Prize.TitleVN,
                TitleEN = x.Prize.TitleEN,

                Rank = x.Rank,
                RewardType = x.RewardType,
                RewardValueMoney = x.RewardValueMoney,
                RewardValueGiftVN = x.RewardValueGiftVN,
                RewardValueGiftEN = x.RewardValueGiftEN,
                AwardedAt = x.AwardedAt ?? DateTime.MinValue
            });

            var totalRecords = await projected.CountAsync();

            var items = await projected
                .OrderByDescending(x => x.AwardedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalRecords);
        }
    }
}
