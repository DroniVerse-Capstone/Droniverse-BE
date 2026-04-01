using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IUserModuleService
{
    Task<UserModuleResponseDTO> CreateUserModuleAsync(CreateUserModuleRequestDTO request);
    Task<PaginationResult<IEnumerable<UserModuleResponseDTO>>> GetMyUserModulesAsync(int pageIndex = 1, int pageSize = 10, bool? isCompleted = null);
    Task<UserModuleResponseDTO> GetMyUserModuleAsync(Guid moduleId);
    Task<UserModuleResponseDTO> UpdateMyUserModuleAsync(Guid moduleId, UpdateUserModuleRequestDTO request);
    Task DeleteMyUserModuleAsync(Guid moduleId);
}
