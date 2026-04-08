using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.Enums;

namespace Droniverse.Academy.Application.IService;

public interface ICodeService
{
    Task<CodeResponseDTO> UpdateCodeAsync(string codeId);
    Task<CodeResponseDTO> DeleteCodeAsync(string codeId);
    Task<CodeResponseDTO> GetCodeAsync(string codeId);
    Task<PaginationResult<IEnumerable<CodeResponseDTO>>> GetAllCodesAsync(CodeSearchRequestDTO requestDTO);
    Task<CodeUsageResponseDTO> EnterCodeAsync(string codeId);
    Task<IEnumerable<string>> CreateCodeAsync(Guid courseId, int quantity, ClubCourseProfit profitType);
}

