using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Droniverse.Shared.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Infrastructure.Repositories
{
    internal class LevelRepository : MySqlRepository<Level>, ILevelRepository
    {
        public LevelRepository(MySqlDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SimpleLevelResponse>> GetLevelsBulkAsync(
            IEnumerable<Guid> levelIds,
            CancellationToken cancellationToken = default)
        {
            var distinctIds = levelIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToArray();

            if (distinctIds.Length == 0)
                return [];

            return await _dbSet
                .AsNoTracking()
                .Where(x => distinctIds.Contains(x.LevelID))
                .Select(x => new SimpleLevelResponse
                {
                    LevelId = x.LevelID,
                    Name = x.Name,
                    LevelNumber = x.LevelNumber,
                    DroneInfo = new SimpleDroneResponse
                    {
                        DroneId = x.Drone.DroneID,
                        DroneNameVN = x.Drone.DroneNameVN,
                        DroneNameEN = x.Drone.DroneNameEN
                    }
                })
                .ToListAsync(cancellationToken);
        }
    }
}
