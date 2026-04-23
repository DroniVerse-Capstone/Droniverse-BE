using Droniverse.Academy.Domain.Entities;
using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Domain.IRepository;

public interface IVRSimulatorRepository : IRepository<VRSimulator>
{
    Task<SimpleVRSimulatorResponse?> GetSimpleVRResponse(Guid vrSimulatorId);
    Task<IEnumerable<SimpleVRSimulatorResponse>> GetSimpleVRResponsesByIdsAsync(IEnumerable<Guid> vrSimulatorIds);
}
