using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;

namespace Droniverse.Academy.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public ReportService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<ReportResponseDTO> CreateReportAsync(CreateReportRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("Nội dung báo cáo là bắt buộc.");

        var lab = await _unitOfWork.Labs.GetByIdAsync(request.LabID);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        var report = _mapper.Map<Report>(request);
        report.ReportID = Guid.NewGuid();
        report.UserID = _currentUser.UserId;
        report.ResponseVN = string.Empty;
        report.ResponseEN = string.Empty;

        await _unitOfWork.Reports.AddAsync(report);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ReportResponseDTO>(report);
    }

    public async Task<PaginationResult<IEnumerable<ReportResponseDTO>>> GetReportsAsync(int pageIndex = 1, int pageSize = 10, Guid? labId = null, Guid? userId = null)
    {
        var result = await _unitOfWork.Reports.GetAllAsync(
            filter: r => (!labId.HasValue || r.LabID == labId.Value)
                      && (!userId.HasValue || r.UserID == userId.Value),
            orderBy: q => q.OrderByDescending(x => x.ReportID),
            pageIndex: pageIndex,
            pageSize: pageSize);

        var mapped = _mapper.Map<IEnumerable<ReportResponseDTO>>(result.Data);
        return new PaginationResult<IEnumerable<ReportResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<PaginationResult<IEnumerable<ReportResponseDTO>>> GetMyReportsAsync(int pageIndex = 1, int pageSize = 10)
    {
        var userId = _currentUser.UserId;

        var result = await _unitOfWork.Reports.GetAllAsync(
            filter: r => r.UserID == userId,
            orderBy: q => q.OrderByDescending(x => x.ReportID),
            pageIndex: pageIndex,
            pageSize: pageSize);

        var mapped = _mapper.Map<IEnumerable<ReportResponseDTO>>(result.Data);
        return new PaginationResult<IEnumerable<ReportResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<ReportResponseDTO> GetReportByIdAsync(Guid reportId)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
        if (report == null)
            throw new BaseException("Không tìm thấy report.", "NOT_FOUND");

        if (!HasManagerPermission() && report.UserID != _currentUser.UserId)
            throw new ForbiddenException("Bạn không có quyền truy cập report này.");

        return _mapper.Map<ReportResponseDTO>(report);
    }

    public async Task<ReportResponseDTO> UpdateMyReportAsync(Guid reportId, UpdateReportRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ValidationException("Nội dung báo cáo là bắt buộc.");

        var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
        if (report == null)
            throw new BaseException("Không tìm thấy report.", "NOT_FOUND");

        if (report.UserID != _currentUser.UserId)
            throw new ForbiddenException("Bạn chỉ có thể cập nhật report của chính mình.");

        _mapper.Map(request, report);

        await _unitOfWork.Reports.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

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

    public async Task DeleteMyReportAsync(Guid reportId)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
        if (report == null)
            throw new BaseException("Không tìm thấy report.", "NOT_FOUND");

        if (report.UserID != _currentUser.UserId)
            throw new ForbiddenException("Bạn chỉ có thể xóa report của chính mình.");

        await _unitOfWork.Reports.DeleteAsync(report);
        await _unitOfWork.SaveChangesAsync();
    }

    private bool HasManagerPermission()
    {
        return _currentUser.Roles.Any(role =>
            role == Roles.Admin ||
            role == Roles.SystemManager ||
            role == Roles.ClubManager);
    }
}
