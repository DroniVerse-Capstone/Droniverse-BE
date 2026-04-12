using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Domain.IRepository.SearchSpec;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Droniverse.Shared.DTOs.Request;
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
                query = query.Where(c => c.UsedByUserID != null || c.Status == CodeStatus.Used);
            }
            else if (usageStatus == CodeUsageStatus.UNUSED)
            {
                query = query.Where(c => c.UsedByUserID == null && c.Status != CodeStatus.Used);
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

    public async Task<PaginationResult<IEnumerable<Code>>> GetCodesByClubAsync(
        Guid clubId,
        GetAllCodesByClubSearchRequest request,
        int pageIndex,
        int pageSize)
    {
        IQueryable<Code> query = _dbSet
            .AsNoTracking()
            .Where(c => c.ClubID == clubId);

        if (request.CodeState == CodeState.Used)
        {
            query = query.Where(c =>
                c.UsedByUserID.HasValue &&
                c.UsedByUserID != Guid.Empty);
        }
        else if (request.CodeState == CodeState.UnUse)
        {
            query = query.Where(c =>
                !c.UsedByUserID.HasValue ||
                c.UsedByUserID == Guid.Empty);
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderBy(c => c.ExpireDate)
            .ThenBy(c => c.CodeID)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResult<IEnumerable<Code>>(items, totalRecords, pageIndex, pageSize);
    }

    public async Task<PaginationResult<IEnumerable<Code>>> GetCodesByUserAsync(
        Guid userId,
        int pageIndex,
        int pageSize,
        bool? isUsed = null)
    {
        IQueryable<Code> query = _dbSet
            .AsNoTracking()
            .Where(c => c.OwnedUserID == userId || c.UsedByUserID == userId);

        if (isUsed.HasValue)
        {
            query = isUsed.Value
                ? query.Where(c => c.Status == CodeStatus.Used || c.UsedByUserID != null)
                : query.Where(c => c.UsedByUserID == null && c.Status == CodeStatus.Active);
        }

        var totalRecords = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.CodeID)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResult<IEnumerable<Code>>(items, totalRecords, pageIndex, pageSize);
    }

    public async Task<IEnumerable<Code>> GetByCodeIdsAsync(IEnumerable<string> codeIds)
    {
        var ids = codeIds?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        if (ids.Count == 0)
        {
            return [];
        }

        return await _dbSet
            .Where(c => ids.Contains(c.CodeID))
            .ToListAsync();
    }
}

