using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.Enums;

namespace Droniverse.Academy.Application.IService;

public interface ICodeService
{
    Task<CodeResponseDTO> UpdateCodeAsync(string codeId);
    Task<CodeResponseDTO> DeleteCodeAsync(string codeId);
    Task<CodeResponseDTO> GetCodeAsync(string codeId);
    Task<PaginationResult<IEnumerable<CodeResponseDTO>>> GetAllCodesAsync(CodeSearchRequestDTO requestDTO);
    Task<CodeUsageResponseDTO> EnterCodeAsync(string codeId);
    Task<CreateCodesResponse> CreateCodeAsync(GenerateCodesRequestDTO request);
    Task<ClubCodesResponse> GetCodesByClub(Guid clubId, GetAllCodesByClubSearchRequest request);
    Task<CodeAssignmentResponseDTO> AssignCodeAsync(AssignCodeRequest request);
    Task<BulkCodeAssignmentResponseDTO> BulkAssignCodesAsync(BulkAssignCodesRequest request);
    Task<PaginationResult<IEnumerable<MyCodeResponseDTO>>> GetCodesByUserAsync(Guid userId, GetCodesByUserSearchRequest request);
}

