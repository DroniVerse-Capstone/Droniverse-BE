using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.IService;

public interface IReportService
{
    Task<ReportResponseDTO> CreateReportAsync(CreateReportRequestDTO request);
    Task<PaginationResult<IEnumerable<ReportResponseDTO>>> GetReportsAsync(int pageIndex = 1, int pageSize = 10, Guid? referenceId = null, Guid? userId = null, ReportType? reportType = null);
    Task<PaginationResult<IEnumerable<ReportResponseDTO>>> GetMyReportsAsync(int pageIndex = 1, int pageSize = 10, ReportType? reportType = null);
    Task<ReportResponseDTO> GetReportByIdAsync(Guid reportId);
    Task<ReportResponseDTO> UpdateMyReportAsync(Guid reportId, UpdateReportRequestDTO request);
    Task<ReportResponseDTO> RespondReportAsync(Guid reportId, RespondReportRequestDTO request);
    Task DeleteMyReportAsync(Guid reportId);
}
