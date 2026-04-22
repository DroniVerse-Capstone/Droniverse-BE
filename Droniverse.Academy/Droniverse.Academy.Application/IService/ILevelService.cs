using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.IService
{
    public interface ILevelService
    {
        Task<IEnumerable<LevelMiniResponse>> GetLevelByDroneAsync(Guid droneId);

        Task<int> ReplaceLevelCoursesAsync(Guid levelId, IEnumerable<Guid>? courseIds, CancellationToken cancellationToken = default);
    }
}
