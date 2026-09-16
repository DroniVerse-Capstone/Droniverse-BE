using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;
using Droniverse.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Identity.Infrastructure.Repositories
{
    public class UserInfoRepository : Repository<UserInfo>, IUserInfoRepository
    {
        public UserInfoRepository(IdentityDbContext context) : base(context)
        {
        }

        public async Task<(int TotalItems, List<SimpleUserReponse> Users)> SearchUsersWithPaginationAsync(
                 string? fullName,
                 string? email,
                 List<Guid>? userIds,
                 int pageIndex,
                 int pageSize)
        {
            var query = _dbSet
                .Include(x => x.Account)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                var keyword = fullName.Trim();

                query = query.Where(x =>
                    x.FirstName.Contains(keyword) ||
                    x.LastName.Contains(keyword) ||
                    (x.FirstName + " " + x.LastName).Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var keyword = email.Trim();

                query = query.Where(x =>
                    x.Account.Email.Contains(keyword));
            }

            if (userIds != null && userIds.Any())
            {
                query = query.Where(x => userIds.Contains(x.UserID));
            }

            var totalItems = await query.CountAsync();

            var users = await query
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SimpleUserReponse
                {
                    UserId = x.UserID,
                    FullName = x.FirstName + " " + x.LastName,
                    Email = x.Account.Email,
                    AvatarUrl = x.ImageUrl
                })
                .ToListAsync();

            return (totalItems, users);
        }
    }
}
