using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface IModuleService
{
    Task<ModuleClientViewDTO> CreateModuleAsync(Guid courseId, Guid versionId, CreateModuleRequestDTO request);
    Task<IEnumerable<ModuleClientViewDTO>> GetModulesAsync(Guid courseId, Guid versionId);
    Task<ModuleClientViewDTO> GetModuleByIdAsync(Guid courseId, Guid versionId, Guid moduleId);
    Task<ModuleClientViewDTO> UpdateModuleAsync(Guid courseId, Guid versionId, Guid moduleId, UpdateModuleRequestDTO request);
    Task DeleteModuleAsync(Guid courseId, Guid versionId, Guid moduleId);
    Task<IEnumerable<ModuleClientViewDTO>> ReorderModulesAsync(Guid courseId, Guid versionId, ReorderModulesRequestDTO request);
}
