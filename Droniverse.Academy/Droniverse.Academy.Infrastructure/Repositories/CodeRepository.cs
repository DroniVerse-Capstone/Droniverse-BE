using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Domain.IRepository.SearchSpec;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CodeRepository : MySqlRepository<Code>, ICodeRepository
{
    public CodeRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<PaginationResult<IEnumerable<Code>>> GetAllCodesAsync(
        ICodeSearchSpec requestDTO,
        int pageIndex, int pageSize)
    {
        IQueryable<Code> query = _dbSet
            .Include(c => c.CodeUsages)
            .AsNoTracking();

        // Lọc theo Status
        if (requestDTO.Status != null)
        {
            var status = requestDTO.Status.Value;
            query = query.Where(c => c.Status == status);
        }

        // Lọc theo CodeUsageStatus
        if (requestDTO.CodeUsageStatus != null)
        {
            var usageStatus = requestDTO.CodeUsageStatus.Value;
            if (usageStatus == CodeUsageStatus.USED)
            {
                query = query.Where(c => c.CodeUsages.Any());
            }
            else if (usageStatus == CodeUsageStatus.UNUSED)
            {
                query = query.Where(c => !c.CodeUsages.Any());
            }
        }

        // Đếm tổng số records trước khi phân trang
        int totalRecords = await query.CountAsync();

        // Phân trang
        var codes = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResult<IEnumerable<Code>>(codes, totalRecords, pageIndex, pageSize);
    }
}

