using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.IService.Mongo;

namespace Droniverse.Academy.Application.Services.Duplication;

public class LabContentSyncService : ILabContentSyncService
{
    private readonly ILabContentService _labContentService;

    public LabContentSyncService(ILabContentService labContentService)
    {
        _labContentService = labContentService;
    }

    public async Task SyncAsync(IReadOnlyCollection<(Guid SourceLabId, Guid NewLabId)> syncQueue)
    {
        if (syncQueue.Count == 0)
            return;

        var processedLabIds = new List<Guid>(syncQueue.Count);
        try
        {
            foreach (var (sourceLabId, newLabId) in syncQueue)
            {
                await SyncSingleAsync(sourceLabId, newLabId);
                processedLabIds.Add(newLabId);
            }
        }
        catch
        {
            await CleanupAsync(processedLabIds);
            throw;
        }
    }

    public async Task CleanupAsync(IEnumerable<Guid> newLabIds)
    {
        foreach (var newLabId in newLabIds.Distinct())
        {
            await _labContentService.DeleteByLabIdAsync(newLabId);
        }
    }

    private async Task SyncSingleAsync(Guid sourceLabId, Guid newLabId)
    {
        var sourceContent = await _labContentService.GetByLabIdAsync(sourceLabId);
        await _labContentService.CreateEmptyAsync(newLabId);

        if (sourceContent == null)
            return;

        await _labContentService.UpdateByLabIdAsync(newLabId, new UpdateLabContentRequestDTO
        {
            Environment = sourceContent.Environment
        });
    }
}
