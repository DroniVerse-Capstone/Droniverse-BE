using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Abstractions;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Services;


public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly IMapper _mapper;
    public CourseService(IUnitOfWork unitOfWork, IClock clock, ICurrentUser current, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUser = current;
        _mapper = mapper;
    }

    public async Task<CourseDetailResponseDTO> CreateCourseAsync()
    {
        var course = new Course
        {
            CourseID = Guid.NewGuid(),
            CourseVersions = new List<CourseVersion>(),
            CreateBy = _currentUser.UserId,
            CreateAt = _clock.Now
        };

        await _unitOfWork.Courses.AddAsync(course);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<CourseDetailResponseDTO>(course);
    }

    public async Task DeleteCourseAsync(Guid courseId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException($"Course {courseId} not found.", "NOT_FOUND");

        course.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PaginationResult<IEnumerable<CourseResponseDTO>>> GetAllCoursesActiveAsync(int pageIndex, int pageSize, string? search = null)
    {
        Expression<Func<Course, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            filter = c => c.CourseVersions.Any(v =>
                v.Status == CourseVersionStatus.ACTIVE &&
                (
                    (v.TitleEN != null && v.TitleEN.Contains(s)) ||
                    (v.TitleVN != null && v.TitleVN.Contains(s))
                ));
        }

        var result = await _unitOfWork.Courses
            .GetAllWithActiveVersionAsync(filter, null, pageIndex, pageSize);

        var mapped = result.Data.Select(c => _mapper.Map<CourseResponseDTO>(c)).ToList();
        return new PaginationResult<IEnumerable<CourseResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<PaginationResult<IEnumerable<CourseDetailResponseDTO>>> GetAllCoursesAllAsync(int pageIndex, int pageSize, string? search = null)
    {
        Expression<Func<Course, bool>>? filter = null;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            filter = c => c.CourseVersions.Any(v =>
                (v.TitleEN != null && v.TitleEN.Contains(s)) ||
                (v.TitleVN != null && v.TitleVN.Contains(s)));
        }

        var result = await _unitOfWork.Courses
            .GetAllWithAllVersionsAsync(filter, null, pageIndex, pageSize);

        var mapped = result.Data.Select(c => _mapper.Map<CourseDetailResponseDTO>(c)).ToList();
        return new PaginationResult<IEnumerable<CourseDetailResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<CourseResponseDTO> GetCourseByIdActiveAsync(Guid courseId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithActiveVersionAsync(courseId);
        if (course == null)
            throw new BaseException($"Course {courseId} not found.", "NOT_FOUND");

        return _mapper.Map<CourseResponseDTO>(course);
    }

    public async Task<CourseDetailResponseDTO> GetCourseByIdAllAsync(Guid courseId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException($"Course {courseId} not found.", "NOT_FOUND");

        return _mapper.Map<CourseDetailResponseDTO>(course);
    }

    public async Task PublishCourseAsync(Guid courseId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException($"Course {courseId} not found.", "NOT_FOUND");

        course.Publish();

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnpublishCourseAsync(Guid courseId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException($"Course {courseId} not found.", "NOT_FOUND");

        course.Unpublish();

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<CourseResponseDTO>> GetCoursesByIdsAsync(IEnumerable<Guid> courseIds)
    {
        var ids = courseIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0)
            return [];

        var result = await _unitOfWork.Courses.GetAllWithAllVersionsAsync(
            filter: c => ids.Contains(c.CourseID),
            pageIndex: 1,
            pageSize: ids.Count);

        var data = result.Data
            .Select(c => _mapper.Map<CourseResponseDTO>(c))
            .ToList();

        return data
            .OrderBy(c => ids.IndexOf(c.CourseID))
            .ToList();
    }
}
