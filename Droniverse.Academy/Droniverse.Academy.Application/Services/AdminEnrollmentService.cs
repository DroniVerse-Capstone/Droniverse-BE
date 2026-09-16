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
using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Services;

public class AdminEnrollmentService : IAdminEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserLookupService _userLookupService;

    public AdminEnrollmentService(IUnitOfWork unitOfWork, IMapper mapper, IUserLookupService userLookupService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userLookupService = userLookupService;
    }

    public async Task<PaginationResult<IEnumerable<CoursesEnrollmentResponse>>> GetEnrollmentsAsync(int pageIndex = 1, int pageSize = 10, Guid? userId = null, Guid? courseVersionId = null, Guid? droneId = null, Guid? levelId = null, Guid? clubId = null, EnrollStatus? status = null)
    {
        Expression<Func<Enrollment, bool>> filter = x => true;

        if (userId.HasValue)
        {
            var value = userId.Value;
            filter = filter.And(x => x.UserID == value);
        }

        if (courseVersionId.HasValue)
        {
            var value = courseVersionId.Value;
            filter = filter.And(x => x.CourseVersionID == value);
        }

        if (droneId.HasValue)
        {
            var value = droneId.Value;
            filter = filter.And(x => x.Course.DroneID == value);
        }

        if (levelId.HasValue)
        {
            var value = levelId.Value;
            filter = filter.And(x => x.Course.LevelID == value);
        }

        if (clubId.HasValue)
        {
            var value = clubId.Value;
            filter = filter.And(x => x.ClubID == value);
        }

        if (status.HasValue)
        {
            var value = status.Value;
            filter = filter.And(x => x.Status == value);
        }

        var result = await _unitOfWork.Enrollments.GetAllAsync(
            filter: filter,
            pageIndex: pageIndex,
            pageSize: pageSize,
            includeProperties: "Course.Level,CourseVersion",
            orderBy: q => q.OrderByDescending(x => x.EnrollDate));

        var userLookup = await _userLookupService.BuildUserLookupAsync(result.Data.Select(x => x.UserID));

        var mapped = result.Data.Select(enrollment =>
        {
            userLookup.TryGetValue(enrollment.UserID, out var user);

            return new CoursesEnrollmentResponse
            {
                EnrollmentId = enrollment.EnrollmentID,
                CourseId = enrollment.CourseID,
                CourseVersionId = enrollment.CourseVersionID,
                CourseNameVN = enrollment.CourseVersion.TitleVN,
                CourseNameEN = enrollment.CourseVersion.TitleEN,
                ImageUrl = enrollment.CourseVersion.ImageUrl,
                EstimatedDuration = enrollment.CourseVersion.EstimatedDuration,
                Progress = enrollment.Progress,
                EnrollStatus = enrollment.Status,
                Level = enrollment.Course.Level == null
                    ? null
                    : new LevelMiniResponse
                    {
                        LevelID = enrollment.Course.Level.LevelID,
                        LevelNumber = enrollment.Course.Level.LevelNumber,
                        Name = enrollment.Course.Level.Name
                    },
                User = user
            };
        }).ToList();

        return new PaginationResult<IEnumerable<CoursesEnrollmentResponse>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<EnrollmentResponseDTO> GetEnrollmentByIdAsync(Guid enrollmentId)
    {
        var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
        if (enrollment == null)
            throw new BaseException("Không tìm thấy enrollment.", "NOT_FOUND");

        return _mapper.Map<EnrollmentResponseDTO>(enrollment);
    }

    public async Task<EnrollmentResponseDTO> UpdateEnrollmentAsync(Guid enrollmentId, AdminUpdateEnrollmentRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Progress.HasValue && request.Progress is < 0 or > 100)
            throw new ValidationException("Progress phải nằm trong khoảng từ 0 đến 100.");

        if (!request.Progress.HasValue && !request.LastAccessDate.HasValue && !request.ExpireDate.HasValue && !request.Status.HasValue)
            throw new ValidationException("Cần ít nhất một trường để cập nhật enrollment.");

        var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
        if (enrollment == null)
            throw new BaseException("Không tìm thấy enrollment.", "NOT_FOUND");

        if (request.Progress.HasValue)
            enrollment.SetProgress(request.Progress.Value);

        if (request.LastAccessDate.HasValue)
            enrollment.LastAccessDate = request.LastAccessDate.Value;

        if (request.ExpireDate.HasValue)
            enrollment.ExpireDate = request.ExpireDate.Value;

        if (request.Status.HasValue)
            enrollment.TransitionTo(request.Status.Value);

        await _unitOfWork.Enrollments.UpdateAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<EnrollmentResponseDTO>(enrollment);
    }

    public async Task DeleteEnrollmentAsync(Guid enrollmentId)
    {
        var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
        if (enrollment == null)
            throw new BaseException("Không tìm thấy enrollment.", "NOT_FOUND");

        await _unitOfWork.Enrollments.DeleteAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();
    }
}
