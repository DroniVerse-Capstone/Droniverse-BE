using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ICodeService
{
    Task<IEnumerable<string>> CreateCodeAsync(Guid courseId, int quantity);
    Task<CodeResponseDTO> UpdateCodeAsync(string codeId);
    Task<CodeResponseDTO> DeleteCodeAsync(string codeId);
    Task<CodeResponseDTO> GetCodeAsync(string codeId);
    Task<PaginationResult<IEnumerable<CodeResponseDTO>>> GetAllCodesAsync();
    Task<CodeUsageResponseDTO> EnterCodeAsync(string codeId);
}

