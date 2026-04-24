using AutoMapper.QueryableExtensions;
using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
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

    public async Task<PaginationResult<IEnumerable<UserResponse>>> GetAllUsersAsync(
        IUserSearchSpecification spec,
        int pageIndex,
        int pageSize)
    {
        IQueryable<Account> query = _dbSet
            .AsNoTracking()
            .Include(a => a.UserInfo)
            .Include(a => a.Role);

        // Filter Username
        if (!string.IsNullOrWhiteSpace(spec?.Username))
        {
            var usernameTerm = spec.Username.Trim();
            query = query.Where(a => a.Username.Contains(usernameTerm));
        }

        // Filter Email
        if (!string.IsNullOrWhiteSpace(spec?.Email))
        {
            var emailTerm = spec.Email.Trim();
            query = query.Where(a => a.Email.Contains(emailTerm));
        }

        // Filter RoleName
        if (spec?.RoleName.HasValue == true)
        {
            var roleName = spec.RoleName.ToString();
            query = query.Where(a => a.Role.RoleName == roleName);
        }

        // Sorting
        if (spec?.SortDirection.HasValue == true)
        {
            query = spec.SortDirection == SortDirection.Desc
                ? query.OrderByDescending(a => a.Username)
                : query.OrderBy(a => a.Username);
        }
        else
        {
            query = query.OrderBy(a => a.Username);
        }

        var totalRecords = await query.CountAsync();
        var skip = (pageIndex - 1) * pageSize;
        var users = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(a => new UserResponse
            {
                UserId = a.UserID,
                Username = a.Username,
                Email = a.Email,
                RoleName = a.Role.RoleName,
                FirstName = a.UserInfo.FirstName,
                LastName = a.UserInfo.LastName,
                DateOfBirth = a.UserInfo.DateOfBirth,
                ImageUrl = a.UserInfo.ImageUrl,
                Gender = a.UserInfo.Gender,
            })
            .ToListAsync();

        return new PaginationResult<IEnumerable<UserResponse>>(
            users,
            totalRecords,
            pageIndex,
            pageSize);
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
                DateOfBirth = a.UserInfo.DateOfBirth,
                ImageUrl = a.UserInfo.ImageUrl,
                Gender = a.UserInfo.Gender,
                Phone = a.UserInfo.Phone,
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<Guid>> GetUsersByUserInfoAsync(
    string? searchName,
    SortDirection sortDirection)
    {
        IQueryable<Account> query = _dbSet
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var terms = searchName
                .Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (terms.Length == 1)
            {
                var term = terms[0];
                var pattern = $"%{term}%";
                query = query.Where(a =>
                    EF.Functions.Like(a.UserInfo.FirstName, pattern) ||
                    EF.Functions.Like(a.UserInfo.LastName, pattern));
            }
            else
            {
                var first = terms[0];
                var last = terms[^1];
                var firstPattern = $"%{first}%";
                var lastPattern = $"%{last}%";

                query = query.Where(a =>
                    (EF.Functions.Like(a.UserInfo.FirstName, firstPattern) && EF.Functions.Like(a.UserInfo.LastName, lastPattern)) ||
                    (EF.Functions.Like(a.UserInfo.FirstName, lastPattern) && EF.Functions.Like(a.UserInfo.LastName, firstPattern)));
            }
        }

        query = sortDirection == SortDirection.Desc
            ? query.OrderByDescending(a => a.UserInfo.FirstName).ThenByDescending(a => a.UserInfo.LastName)
            : query.OrderBy(a => a.UserInfo.FirstName).ThenBy(a => a.UserInfo.LastName);

        return await query
            .Select(a => a.UserID)
            .ToListAsync();
    }
}
