using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class ClubPolicyRepository : MySqlRepository<ClubPolicy>, IClubPolicyRepository
{
    public ClubPolicyRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<PaginationResult<IEnumerable<ClubPolicy>>> GetAll(
        int currentPage = 1,
        int pageSize = 5)
    {
        currentPage = currentPage < 1 ? 1 : currentPage;
        pageSize = pageSize < 5 ? 5 : (pageSize > 20 ? 20 : pageSize);

        var query = _context.Set<ClubPolicy>()
            .AsNoTracking()
            .Include(cp => cp.Club)
            .AsQueryable();

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResult<IEnumerable<ClubPolicy>>(data, totalRecords, currentPage, pageSize);
    }

    
}

