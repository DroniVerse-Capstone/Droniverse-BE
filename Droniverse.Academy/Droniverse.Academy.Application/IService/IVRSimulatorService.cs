using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Enums;

namespace Droniverse.Academy.Application.IService;

public interface IVRSimulatorService
{
    Task<VRSimulatorClientViewDTO> CreateVRSimulatorAsync(CreateVRSimulatorRequestDTO request);
    Task<LessonClientViewDTO> CreateLessonFromVRSimulatorAsync(Guid vrSimulatorId, CreateVRSimulatorLessonRequestDTO request);
    Task<PaginationResult<IEnumerable<VRSimulatorClientViewDTO>>> GetVRSimulatorsAsync(int pageIndex = 1, int pageSize = 10, string? search = null, VRSimulatorType? type = null);
    Task<VRSimulatorClientViewDTO> GetVRSimulatorByIdAsync(Guid vrSimulatorId);
    Task<VRSimulatorClientViewDTO> UpdateVRSimulatorAsync(Guid vrSimulatorId, UpdateVRSimulatorRequestDTO request);
    Task<SimpleVRSimulatorResponse> GetSimpleVRSimulator(Guid vrSimulatorId);
    Task<IEnumerable<SimpleVRSimulatorResponse>> GetVRSimulatorByIds(IEnumerable<Guid> vrSimulatorIds);

    Task DeleteVRSimulatorAsync(Guid vrSimulatorId);
}
