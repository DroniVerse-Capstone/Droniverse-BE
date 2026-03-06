using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Droniverse.Identity.Infrastructure.Repositories;
public class UserRepository : Repository<Account>, IUserRepository
{
    public UserRepository(IdentityDbContext context) : base(context)
    {
    }

    public new async Task<IEnumerable<Account>> GetAll()
    {
        return await _dbSet
            .Include(a => a.UserInfo)
            .Include(a => a.Role)
            .ToListAsync();
    }

    public new async Task<Account?> GetByCondition(Expression<Func<Account, bool>> expression)
    {
        return await _dbSet
            .Include(a => a.UserInfo)
            .Include(a => a.Role)
            .Where(expression)
            .FirstOrDefaultAsync();
    }   

    public new async Task<IEnumerable<Account>> GetManyByCondition(Expression<Func<Account, bool>> expression)
    {
        return await _dbSet
            .Include(a => a.UserInfo)
            .Include(a => a.Role)
            .Where(expression)
            .ToListAsync();
    }
}
