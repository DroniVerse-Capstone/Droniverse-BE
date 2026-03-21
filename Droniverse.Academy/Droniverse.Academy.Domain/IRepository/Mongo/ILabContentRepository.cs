using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Domain.IRepository.Mongo;

public interface ILabContentRepository
{
    Task<LabContent> CreateAsync(LabContent labContent, CancellationToken cancellationToken = default);
    Task<IEnumerable<LabContent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LabContent?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<LabContent?> UpdateAsync(string id, LabContent labContent, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
