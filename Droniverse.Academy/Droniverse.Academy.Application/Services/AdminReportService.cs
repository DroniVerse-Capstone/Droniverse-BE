using AutoMapper;
using Droniverse.Academy.Application.Common.Extensions;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class AdminReportService : IAdminReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserLookupService _userLookupService;

    public AdminReportService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IUserLookupService userLookupService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _userLookupService = userLookupService;
    }

    public async Task<PaginationResult<IEnumerable<ReportResponseDTO>>> GetReportsAsync(int pageIndex = 1, int pageSize = 10, Guid? referenceId = null, Guid? userId = null, ReportType? reportType = null)
    {
        var result = await _unitOfWork.Reports.GetAllAsync(
            filter: x => (!referenceId.HasValue || x.ReferenceID == referenceId.Value)
                      && (!userId.HasValue || x.UserID == userId.Value)
                      && (!reportType.HasValue || x.ReportType == reportType.Value),
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.ReportID));

        var reports = result.Data.ToList();
        var mapped = _mapper.Map<List<ReportResponseDTO>>(reports);
        await PopulateUsersAsync(reports, mapped);
        return new PaginationResult<IEnumerable<ReportResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<ReportResponseDTO> GetReportByIdAsync(Guid reportId)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
        if (report == null)
            throw new BaseException("Không tìm thấy report.", "NOT_FOUND");

        var response = _mapper.Map<ReportResponseDTO>(report);
        await PopulateUsersAsync(report, response);
        return response;
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
        report.Responser = _currentUser.UserId;

        await _unitOfWork.Reports.UpdateAsync(report);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<ReportResponseDTO>(report);
        await PopulateUsersAsync(report, response);
        return response;
    }

    public async Task DeleteReportAsync(Guid reportId)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(reportId);
        if (report == null)
            throw new BaseException("Không tìm thấy report.", "NOT_FOUND");

        await _unitOfWork.Reports.DeleteAsync(report);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task PopulateUsersAsync(Report report, ReportResponseDTO response)
    {
        var userIds = new Guid?[] { report.UserID, report.Responser }.ToDistinctValidIds();
        var userLookup = await _userLookupService.BuildUserLookupAsync(userIds);

        if (userLookup.TryGetValue(report.UserID, out var reporter))
        {
            response.User = reporter;
        }

        if (report.Responser.HasValue && userLookup.TryGetValue(report.Responser.Value, out var responser))
        {
            response.ResponserUser = responser;
        }
    }

    private async Task PopulateUsersAsync(IReadOnlyList<Report> reports, IList<ReportResponseDTO> responses)
    {
        var userIds = reports
            .SelectMany(r => new Guid?[] { r.UserID, r.Responser })
            .ToDistinctValidIds();

        var userLookup = await _userLookupService.BuildUserLookupAsync(userIds);

        foreach (var (report, response) in reports.Zip(responses))
        {
            if (userLookup.TryGetValue(report.UserID, out var reporter))
            {
                response.User = reporter;
            }

            if (report.Responser.HasValue && userLookup.TryGetValue(report.Responser.Value, out var responser))
            {
                response.ResponserUser = responser;
            }
        }
    }
}
