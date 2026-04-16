using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.DTO.Extension;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public EnrollmentService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<EnrollmentResponseDTO> CreateEnrollmentAsync(CreateEnrollmentRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var courseVersion = await _unitOfWork.CourseVersions.GetByIdAsync(request.CourseVersionID);
        if (courseVersion == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        var userId = _currentUser.UserId;

        var existing = await _unitOfWork.Enrollments.GetByConditionAsync(
            x => x.UserID == userId && x.CourseVersionID == request.CourseVersionID);

        if (existing != null)
            throw new ValidationException("Người dùng đã đăng ký phiên bản khóa học này.");

        var enrollment = _mapper.Map<Enrollment>(request);
        enrollment.EnrollmentID = Guid.NewGuid();
        enrollment.CourseID = courseVersion.CourseID;
        enrollment.UserID = userId;
        enrollment.EnrollDate = _clock.Now;
        enrollment.LastAccessDate = _clock.Now;
        enrollment.ExpireDate = _clock.Now.AddMonths(6);
        enrollment.Progress = 0;
        enrollment.Status = EnrollStatus.ACTIVE;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _unitOfWork.Enrollments.AddAsync(enrollment);
            await CreateUserModulesAsync(courseVersion, enrollment.EnrollDate);
            await _unitOfWork.SaveChangesAsync();
        });

        return _mapper.Map<EnrollmentResponseDTO>(enrollment);
    }

    public async Task<PaginationResult<IEnumerable<EnrollmentResponseDTO>>> GetMyEnrollmentsAsync(int pageIndex = 1, int pageSize = 10, EnrollStatus? status = null)
    {
        var userId = _currentUser.UserId;

        var result = await _unitOfWork.Enrollments.GetAllAsync(
            status.HasValue
                ? x => x.UserID == userId && x.Status == status.Value
                : x => x.UserID == userId,
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.EnrollDate));

        var mapped = result.Data.Select(x => _mapper.Map<EnrollmentResponseDTO>(x)).ToList();
        return new PaginationResult<IEnumerable<EnrollmentResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<EnrollmentResponseDTO> GetMyEnrollmentByIdAsync(Guid enrollmentId)
    {
        var enrollment = await GetMyEnrollmentEntityOrThrowAsync(enrollmentId);

        return _mapper.Map<EnrollmentResponseDTO>(enrollment);
    }

    public async Task<EnrollmentResponseDTO> GetMyEnrollmentByClubAndCourseVersionAsync(Guid clubId, Guid courseVersionId)
    {
        if (clubId == Guid.Empty)
            throw new ValidationException("ClubId không hợp lệ.");

        if (courseVersionId == Guid.Empty)
            throw new ValidationException("CourseVersionId không hợp lệ.");

        var userId = _currentUser.UserId;
        var enrollment = await _unitOfWork.Enrollments.GetByConditionAsync(
            x => x.UserID == userId && x.ClubID == clubId && x.CourseVersionID == courseVersionId);

        if (enrollment == null)
            throw new BaseException("Không tìm thấy enrollment.", "NOT_FOUND");

        return _mapper.Map<EnrollmentResponseDTO>(enrollment);
    }

    public async Task<EnrollmentResponseDTO> UpdateMyEnrollmentAsync(Guid enrollmentId, UpdateEnrollmentRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!request.Progress.HasValue && !request.LastAccessDate.HasValue && !request.ExpireDate.HasValue && !request.Status.HasValue)
            throw new ValidationException("Cần ít nhất một trường để cập nhật enrollment.");

        if (request.Status.HasValue)
            throw new ValidationException("Status được xác định tự động theo Progress.");

        if (request.Progress.HasValue && request.Progress is < 0 or > 100)
            throw new ValidationException("Progress phải nằm trong khoảng từ 0 đến 100.");

        var enrollment = await GetMyEnrollmentEntityOrThrowAsync(enrollmentId);

        if (request.Progress.HasValue)
        {
            enrollment.Progress = request.Progress.Value;
            enrollment.Status = enrollment.Progress >= 100 ? EnrollStatus.COMPLETED : EnrollStatus.ACTIVE;
        }

        enrollment.LastAccessDate = request.LastAccessDate ?? _clock.Now;

        if (request.ExpireDate.HasValue)
            enrollment.ExpireDate = request.ExpireDate.Value;

        await _unitOfWork.Enrollments.UpdateAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<EnrollmentResponseDTO>(enrollment);
    }

    public async Task DeleteMyEnrollmentAsync(Guid enrollmentId)
    {
        var enrollment = await GetMyEnrollmentEntityOrThrowAsync(enrollmentId);

        await _unitOfWork.Enrollments.DeleteAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PaginationResult<IEnumerable<CoursesEnrollmentResponse>>> GetCoursesOfUser(Guid clubId, UserEnrollmentSearchRequest request)
    {
        if (clubId == Guid.Empty)
            throw new ValidationException("ClubId không hợp lệ.");

        request ??= new UserEnrollmentSearchRequest();
        var currentUserId = _currentUser.UserId;
        EnrollStatus? enrollmentStatus = request.EnrollmentStatus switch
        {
            UserEnrollment.ACTIVE => EnrollStatus.ACTIVE,
            UserEnrollment.COMPLETED => EnrollStatus.COMPLETED,
            _ => null
        };

        var result = await _unitOfWork.Enrollments.GetCoursesOfUserAsync(
            userId: currentUserId,
            clubId: clubId,
            pageIndex: request.CurrentPage,
            pageSize: request.PageSize,
            level: request.Level,
            courseSearchName: request.CourseSearchName,
            enrollmentStatus: enrollmentStatus);

        var mapped = result.Data.Select(x => new CoursesEnrollmentResponse
        {
            EnrollmentId = x.EnrollmentId,
            CourseId = x.CourseId,
            CourseVersionId = x.CourseVersionId,
            CourseNameVN = x.CourseNameVN,
            CourseNameEN = x.CourseNameEN,
            ImageUrl = x.ImageUrl,
            Level = x.Level,
            EstimatedDuration = x.EstimatedDuration,
            Progress = x.Progress,
            EnrollStatus = x.EnrollStatus
        }).ToList();

        return new PaginationResult<IEnumerable<CoursesEnrollmentResponse>>(
            mapped,
            result.TotalRecords,
            result.PageIndex,
            result.PageSize);
    }

    private async Task<Enrollment> GetMyEnrollmentEntityOrThrowAsync(Guid enrollmentId)
    {
        var userId = _currentUser.UserId;

        var enrollment = await _unitOfWork.Enrollments.GetByConditionAsync(
            x => x.EnrollmentID == enrollmentId && x.UserID == userId);

        if (enrollment == null)
            throw new BaseException("Không tìm thấy enrollment.", "NOT_FOUND");

        return enrollment;
    }

    private async Task CreateUserModulesAsync(CourseVersion courseVersion, DateTime enrollDate)
    {
        var modulesResult = await _unitOfWork.Modules.GetAllAsync(
            filter: x => x.CourseVersionID == courseVersion.CourseVersionID,
            orderBy: q => q.OrderBy(x => x.ModuleNumber),
            pageIndex: 1,
            pageSize: 10000);

        var userModules = modulesResult.Data
            .Select(module => new UserModule
            {
                UserID = _currentUser.UserId,
                ModuleID = module.ModuleID,
                EnrollDate = enrollDate,
                Progress = 0,
                CompleteDate = null,
                IsCompleted = false
            })
            .ToList();

        if (userModules.Count == 0)
            return;

        await _unitOfWork.UserModules.AddRangeAsync(userModules);
    }

}
