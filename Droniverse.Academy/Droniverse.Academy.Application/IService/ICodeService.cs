using Droniverse.Academy.Application.DTO.Extension;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;

namespace Droniverse.Academy.Application.IService;

public interface ICodeService
{
    Task<CodeResponseDTO> UpdateCodeAsync(string codeId);
    Task<CodeResponseDTO> DeleteCodeAsync(string codeId);
    Task<CodeResponseDTO> GetCodeAsync(string codeId);
    Task<PaginationResult<IEnumerable<CodeResponseDTO>>> GetAllCodesAsync(CodeSearchRequestDTO requestDTO);
    Task<CodeUsageResponseDTO> EnterCodeAsync(Guid clubId, string codeId);
    Task<CreateCodesResponse> CreateCodeAsync(GenerateCodesRequestDTO request);
    Task<ClubCodesResponse> GetCodesByClub(Guid clubId, Guid courseId, GetAllCodesByClubSearchRequest request);
    Task<CodeAssignmentResponseDTO> AssignCodeAsync(AssignCodeRequest request);
    Task<BulkCodeAssignmentResponseDTO> BulkAssignCodesAsync(BulkAssignCodesRequest request);
    Task<PaginationResult<IEnumerable<MyCodeResponseDTO>>> GetCodesByUserAsync(GetCodesByUserSearchRequest request);
    Task<PaginationResult<IEnumerable<SimpleUserReponse>>> GetUsersCode(Guid clubId, Guid courseId, GetUsersNoCodesSearchRequest request);
    Task<CodeResponseDTO> CreateWithAssignCodeAsync(Shared.DTOs.Request.GenerateWithAssignCodeRequestDTO request);
    Task<GetCodeByUsersResponseDTO> GetCodeByUsers(Guid clubId, Guid courseId);
}

