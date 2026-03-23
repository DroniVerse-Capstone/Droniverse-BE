using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IUserLabService
{
    Task<UserLabResponseDTO> CreateUserLabAsync(CreateUserLabRequestDTO request);
    Task<PaginationResult<IEnumerable<UserLabResponseDTO>>> GetMyUserLabsAsync(int pageIndex = 1, int pageSize = 10, bool? isCompleted = null);
    Task<UserLabResponseDTO> GetMyUserLabByIdAsync(Guid userLabId);
    Task<UserLabResponseDTO> UpdateMyUserLabAsync(Guid userLabId, UpdateUserLabRequestDTO request);
    Task DeleteMyUserLabAsync(Guid userLabId);
}
