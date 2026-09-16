using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Droniverse.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class VRSimulatorRepository : MySqlRepository<VRSimulator>, IVRSimulatorRepository
{
    public VRSimulatorRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<SimpleVRSimulatorResponse?> GetSimpleVRResponse(Guid vrSimulatorId)
    {
        return await _dbSet.Where(x => x.VRSimulatorID == vrSimulatorId)
            .Select(x => new SimpleVRSimulatorResponse
            {
                VRSimulatorId = x.VRSimulatorID,
                TitleVN = x.TitleVN,
                TitleEN = x.TitleEN,
                Type = (Droniverse.Shared.Enums.VRSimulatorType)x.Type,
            }).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SimpleVRSimulatorResponse>> GetSimpleVRResponsesByIdsAsync(IEnumerable<Guid> vrSimulatorIds)
    {
        var ids = vrSimulatorIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (ids.Count == 0)
            return [];

        return await _dbSet
            .AsNoTracking()
            .Where(x => ids.Contains(x.VRSimulatorID))
            .Select(x => new SimpleVRSimulatorResponse
            {
                VRSimulatorId = x.VRSimulatorID,
                TitleVN = x.TitleVN,
                TitleEN = x.TitleEN,
                Type = (Droniverse.Shared.Enums.VRSimulatorType)x.Type,
            })
            .ToListAsync();
    }
}
