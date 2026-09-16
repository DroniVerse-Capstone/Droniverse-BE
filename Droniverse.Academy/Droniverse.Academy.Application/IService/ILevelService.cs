using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs;
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

        Task<IEnumerable<SimpleLevelResponse>> GetLevelsBulkAsync(IEnumerable<Guid>? levelIds, CancellationToken cancellationToken = default);

        Task<IEnumerable<LevelPathResponseDTO>> GetLevelPathAsync(Guid droneId, CancellationToken cancellationToken = default);

        Task<int> ReplaceLevelCoursesAsync(Guid levelId, IEnumerable<Guid>? courseIds, CancellationToken cancellationToken = default);
    }
}
