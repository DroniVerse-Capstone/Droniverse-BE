using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public class AdminReportService : IAdminReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminReportService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginationResult<IEnumerable<ReportResponseDTO>>> GetReportsAsync(int pageIndex = 1, int pageSize = 10, Guid? referenceId = null, Guid? userId = null)
    {
        var result = await _unitOfWork.Reports.GetAllAsync(
            filter: x => (!referenceId.HasValue || x.ReferenceID == referenceId.Value)
                      && (!userId.HasValue || x.UserID == userId.Value),
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.ReportID));

        var mapped = _mapper.Map<IEnumerable<ReportResponseDTO>>(result.Data);
        return new PaginationResult<IEnumerable<ReportResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<ReportResponseDTO> GetReportByIdAsync(Guid reportId)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
        if (report == null)
            throw new BaseException("Không tìm thấy report.", "NOT_FOUND");

        return _mapper.Map<ReportResponseDTO>(report);
    }

    public async Task<ReportResponseDTO> RespondReportAsync(Guid reportId, RespondReportRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.ResponseVN) && string.IsNullOrWhiteSpace(request.ResponseEN))
            throw new ValidationException("Phản hồi là bắt buộc.");

        var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
        if (report == null)
            throw new BaseException("Không tìm thấy report.", "NOT_FOUND");

        _mapper.Map(request, report);

        await _unitOfWork.Reports.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ReportResponseDTO>(report);
    }

    public async Task DeleteReportAsync(Guid reportId)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
        if (report == null)
            throw new BaseException("Không tìm thấy report.", "NOT_FOUND");

        await _unitOfWork.Reports.DeleteAsync(report);
        await _unitOfWork.SaveChangesAsync();
    }
}
