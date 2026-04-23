using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.IService;

public interface IVRSimulatorService
{
    Task<VRSimulatorClientViewDTO> CreateVRSimulatorAsync(CreateVRSimulatorRequestDTO request);
    Task<LessonClientViewDTO> CreateLessonFromVRSimulatorAsync(Guid vrSimulatorId, CreateVRSimulatorLessonRequestDTO request);
    Task<IEnumerable<VRSimulatorClientViewDTO>> GetVRSimulatorsAsync();
    Task<VRSimulatorClientViewDTO> GetVRSimulatorByIdAsync(Guid vrSimulatorId);
    Task<VRSimulatorClientViewDTO> UpdateVRSimulatorAsync(Guid vrSimulatorId, UpdateVRSimulatorRequestDTO request);
    Task<SimpleVRSimulatorResponse> GetSimpleVRSimulator(Guid vrSimulatorId);
    Task<IEnumerable<SimpleVRSimulatorResponse>> GetVRSimulatorByIds(IEnumerable<Guid> vrSimulatorIds);

    Task DeleteVRSimulatorAsync(Guid vrSimulatorId);
}
