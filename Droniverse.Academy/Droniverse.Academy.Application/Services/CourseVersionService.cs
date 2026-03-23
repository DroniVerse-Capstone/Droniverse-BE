using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;
using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Services;

public class CourseVersionService : ICourseVersionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public CourseVersionService(IUnitOfWork unitOfWork, ICurrentUserService current, IClock clock, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = current;
        _clock = clock;
        _mapper = mapper;
    }

    public async Task<CourseVersionResponseDTO> CreateCourseVersionAsync(Guid courseId, CreateCourseVersionRequestDTO request)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        // determine next version number
        var nextVersion = 1;
        if (course.CourseVersions != null && course.CourseVersions.Any())
        {
            nextVersion = course.CourseVersions.Max(v => v.Version) + 1;
        }

        var cv = _mapper.Map<CourseVersion>(request);
        cv.CourseVersionID = Guid.NewGuid();
        cv.CourseID = course.CourseID;
        cv.Version = nextVersion;

        await _unitOfWork.CourseVersions.AddAsync(cv);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CourseVersionResponseDTO>(cv);
    }

    public async Task DeleteCourseVersionAsync(Guid courseId, Guid versionId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(v => v.CourseVersionID == versionId && v.CourseID == courseId);
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (course.CurrentVersionID == versionId)
            throw new ValidationException("Không thể xóa phiên bản hiện tại. Vui lòng vô hiệu hóa phiên bản hiện tại trước.");

        if (cv.Status == CourseVersionStatus.ACTIVE)
            throw new ValidationException("Không thể xóa phiên bản đang hoạt động.");

        await _unitOfWork.CourseVersions.DeleteAsync(cv);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CourseVersionResponseDTO> GetCourseVersionByIdAsync(Guid courseId, Guid versionId)
    {
        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(v => v.CourseVersionID == versionId && v.CourseID == courseId, includeProperties: "CourseVersionCategories,RequiredDrones");
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        return _mapper.Map<CourseVersionResponseDTO>(cv);
    }

    public async Task<PaginationResult<IEnumerable<CourseVersionResponseDTO>>> GetCourseVersionsAsync(Guid courseId, int pageIndex, int pageSize, CourseVersionStatus? status = null)
    {
        Expression<Func<CourseVersion, bool>>? filter = v => v.CourseID == courseId;
        if (status.HasValue)
        {
            var s = status.Value;
            filter = v => v.CourseID == courseId && v.Status == s;
        }

        var result = await _unitOfWork.CourseVersions.GetAllAsync(filter, null, pageIndex, pageSize, includeProperties: "CourseVersionCategories,RequiredDrones");
        var mapped = result.Data.Select(v => _mapper.Map<CourseVersionResponseDTO>(v)).ToList();
        return new PaginationResult<IEnumerable<CourseVersionResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task ActivateCourseVersionAsync(Guid courseId, Guid versionId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        var cv = course.CourseVersions.FirstOrDefault(v => v.CourseVersionID == versionId);
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        // deprecate other active versions
        var active = course.CourseVersions
            .Where(v => v.Status == CourseVersionStatus.ACTIVE && v.CourseVersionID != versionId)
            .ToList();
        foreach (var a in active)
        {
            a.Deprecate(_currentUser.UserId, _clock.Now);
        }

        cv.Activate(_currentUser.UserId, _clock.Now);
        // set current version
        course.CurrentVersion = cv;
        course.CurrentVersionID = cv.CourseVersionID;


        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeactivateCourseVersionAsync(Guid courseId, Guid versionId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        var cv = course.CourseVersions.FirstOrDefault(v => v.CourseVersionID == versionId);
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (cv.Status != CourseVersionStatus.ACTIVE)
            throw new ValidationException("Chỉ phiên bản đang hoạt động mới có thể bị vô hiệu hóa.");

        cv.Deprecate(_currentUser.UserId, _clock.Now);

        // deprecated manually -> remove from current version if matched
        if (course.CurrentVersionID == cv.CourseVersionID)
        {
            course.CurrentVersion = null;
            course.CurrentVersionID = null;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CourseVersionResponseDTO> UpdateCourseVersionAsync(Guid courseId, Guid versionId, UpdateCourseVersionRequestDTO request)
    {
        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(v => v.CourseVersionID == versionId && v.CourseID == courseId);
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        cv.UpdateContent(request.TitleVN, request.TitleEN, request.DescriptionVN, request.DescriptionEN, request.ContextVN, request.ContextEN, request.ImageUrl, request.Level, request.EstimatedDuration, _currentUser.UserId, _clock.Now);

        await _unitOfWork.CourseVersions.UpdateAsync(cv);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CourseVersionResponseDTO>(cv);
    }
}
