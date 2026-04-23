using Droniverse.Academy.Domain.Entities;
using System.Collections;

namespace Droniverse.Academy.Domain.IRepository;

public interface IUserLevelRepository : IRepository<UserLevel>
{
    Task<IEnumerable<Guid>> GetUserLevelIdsAsync(Guid userId);
}
