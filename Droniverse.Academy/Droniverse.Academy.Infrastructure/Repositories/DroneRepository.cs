using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Droniverse.Shared.DTOs.Response;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class DroneRepository : MySqlRepository<Drone>, IDroneRepository
{
    public DroneRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DroneResponseDto>> GetDronesByIdsAsync(IEnumerable<Guid> droneIds)
    {
        var distinctIds = droneIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (!distinctIds.Any())
            return [];

        return await _dbSet
            .AsNoTracking()
            .Where(a => distinctIds.Contains(a.DroneID))
            .Select(a => new DroneResponseDto
            {
                DroneID = a.DroneID,
                DroneNameEN = a.DroneNameEN,
                DroneNameVN = a.DroneNameVN,
                DescriptionEN = a.DescriptionEN,
                DescriptionVN = a.DescriptionVN,
                DroneTypeNameEN = a.DroneType.TypeNameEN,
                DroneTypeNameVN = a.DroneType.TypeNameVN,
                DroneTypeID = a.DroneTypeID,
                Height = a.Height,
                Weight = a.Weight,
                ImgURL = a.ImgURL,
                Manufacturer = a.Manufacturer,
                Status = (DroneStatus) a.Status

            })
            .ToListAsync();
    }
}

