using AutoMapper.QueryableExtensions;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;
using Droniverse.Shared.DTOs.Response;
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

    public async Task<IEnumerable<UserResponse>> GetUsersByIdsAsync(IEnumerable<Guid> userIds)
    {
        var distinctIds = userIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (!distinctIds.Any())
            return [];

        return await _dbSet
            .AsNoTracking()
            .Where(a => distinctIds.Contains(a.UserID))
            .Select(a => new UserResponse
            {
                UserId = a.UserID,
                Username = a.Username,
                Email = a.Email,
                RoleName = a.Role.RoleName,
                FirstName = a.UserInfo.FirstName,
                LastName = a.UserInfo.LastName,
                DateOfBirth = a.UserInfo.DateOfBirth
            })
            .ToListAsync();
    }
}
