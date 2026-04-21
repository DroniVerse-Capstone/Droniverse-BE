using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IAdminReportService
{
    Task<PaginationResult<IEnumerable<ReportResponseDTO>>> GetReportsAsync(int pageIndex = 1, int pageSize = 10, Guid? referenceId = null, Guid? userId = null);
    Task<ReportResponseDTO> GetReportByIdAsync(Guid reportId);
    Task<ReportResponseDTO> RespondReportAsync(Guid reportId, RespondReportRequestDTO request);
    Task DeleteReportAsync(Guid reportId);
}
