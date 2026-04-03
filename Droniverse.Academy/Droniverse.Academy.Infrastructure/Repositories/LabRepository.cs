using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Droniverse.Community.Application.DTO.Response;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class LabRepository : MySqlRepository<Lab>, ILabRepository
{
    public LabRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<bool> IsExistAsync(Guid labId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(x => x.LabID == labId, cancellationToken);
    }

    public async Task<IEnumerable<SimpleLabResponse>> GetSimpleLabsByIdsAsync(IEnumerable<Guid> labIds, CancellationToken cancellationToken = default)
    {
        var ids = labIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (ids.Count == 0)
            return [];

        return await _dbSet
            .Where(x => ids.Contains(x.LabID))
            .AsNoTracking()
            .Select(x => new SimpleLabResponse
            {
                LabID = x.LabID,
                LabNameVN = x.NameVN,
                LabNameEN = x.NameEN
            })
            .ToListAsync(cancellationToken);
    }
}

