using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.Enums.SeachRequest;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Droniverse.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Repositories;

internal class ClubAttemptRequestRepository : MySqlRepository<ClubAttemptRequest>, IClubAttemptRequestRepository
{
    public ClubAttemptRequestRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<bool> IsUserInClubAttemptRequest(Guid userID, Guid clubID)
    {
        return await _context.Set<ClubAttemptRequest>()
            .AnyAsync(x => x.RequesterID == userID 
                        && x.ClubID == clubID 
                        && x.Status == ClubAttemptRequestStatus.PENDING);
    }

    public async Task<(IEnumerable<ClubAttemptRequest> Items, int TotalCount)> GetFilteredRequestsAsync(
        Guid clubID,
        ClubAttemptRequestStatus? status,
        DateTime? createdFrom,
        DateTime? createdTo,
        DateTime? processedFrom,
        DateTime? processedTo,
        ClubAttemptRequestSortBy sortBy,
        SortDirection sortDirection,
        int skip,
        int take)
    {
        // Build query with filters
        var query = _context.Set<ClubAttemptRequest>()
            .Where(c => c.ClubID == clubID)
            .Include(r => r.Club)
            .AsNoTracking() // Performance: No tracking since we're reading only
            .AsQueryable();

        // Apply status filter
        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        // Apply CreatedAt range filter
        if (createdFrom.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= createdFrom.Value);
        }

        if (createdTo.HasValue)
        {
            var toDate = createdTo.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(r => r.CreatedAt <= toDate);
        }

        // Apply ProcessedAt range filter
        if (processedFrom.HasValue)
        {
            query = query.Where(r => r.ProcessedAt.HasValue 
                                  && r.ProcessedAt.Value >= processedFrom.Value);
        }

        if (processedTo.HasValue)
        {
            var toDate = processedTo.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(r => r.ProcessedAt.HasValue 
                                  && r.ProcessedAt.Value <= toDate);
        }

        // Get total count BEFORE pagination (optimized - single query)
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, sortBy, sortDirection);

        // Apply pagination
        var items = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (items, totalCount);
    }

    /// <summary>
    /// Apply sorting logic
    /// </summary>
    private static IQueryable<ClubAttemptRequest> ApplySorting(
    IQueryable<ClubAttemptRequest> query,
    ClubAttemptRequestSortBy sortBy,
    SortDirection sortDirection)
    {
        var isAscending = sortDirection == SortDirection.Asc;

        return sortBy switch
        {
            ClubAttemptRequestSortBy.ProcessedAt => isAscending
                ? query.OrderBy(r => r.ProcessedAt)
                : query.OrderByDescending(r => r.ProcessedAt),

            ClubAttemptRequestSortBy.CreatedAt or _ => isAscending
                ? query.OrderBy(r => r.CreatedAt)
                : query.OrderByDescending(r => r.CreatedAt)
        };
    }
}

