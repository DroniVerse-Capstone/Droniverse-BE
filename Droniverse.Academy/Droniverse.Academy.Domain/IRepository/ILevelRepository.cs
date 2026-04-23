using Droniverse.Academy.Domain.Entities;
using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Domain.IRepository
{
    public interface ILevelRepository : IRepository<Level>
    {
        Task<IEnumerable<SimpleLevelResponse>> GetLevelsBulkAsync(IEnumerable<Guid> levelIds, CancellationToken cancellationToken = default);
    }
}
