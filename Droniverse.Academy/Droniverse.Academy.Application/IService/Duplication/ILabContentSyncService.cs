namespace Droniverse.Academy.Application.IService.Duplication;

public interface ILabContentSyncService
{
    Task SyncAsync(IReadOnlyCollection<(Guid SourceLabId, Guid NewLabId)> syncQueue);
    Task CleanupAsync(IEnumerable<Guid> newLabIds);
}
