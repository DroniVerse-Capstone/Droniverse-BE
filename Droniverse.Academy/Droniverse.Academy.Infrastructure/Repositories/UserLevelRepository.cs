using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class UserLevelRepository : MySqlRepository<UserLevel>, IUserLevelRepository
{
    public UserLevelRepository(MySqlDbContext context) : base(context)
    {
    }


    public async Task<IEnumerable<Guid>> GetUserLevelIdsAsync(Guid userId)
    {
        return await _dbSet.Where(ul => ul.UserID == userId).Select(x => x.LevelID).ToListAsync();
    }
}
