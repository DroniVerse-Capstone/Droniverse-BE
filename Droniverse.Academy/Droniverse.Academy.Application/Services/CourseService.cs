using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Services;


public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IMapper _mapper;
    private readonly IUserDisplayNameService _userDisplayNameService;
    public CourseService(IUnitOfWork unitOfWork, IClock clock, ICurrentUserService current, IMapper mapper, IUserDisplayNameService userDisplayNameService)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUser = current;
        _mapper = mapper;
        _userDisplayNameService = userDisplayNameService;
    }

    public async Task<CourseDetailResponseDTO> CreateCourseAsync()
    {
        var course = new Course
        {
            CourseID = Guid.NewGuid(),
            CourseVersions = new List<CourseVersion>()
        };
        course.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Courses.AddAsync(course);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<CourseDetailResponseDTO>(course);
        await PopulateCreatorAsync(response, course.CreateBy);

        return response;
    }

    public async Task DeleteCourseAsync(Guid courseId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        if (course.Status != CourseStatus.DRAFT && course.Status != CourseStatus.UNPUBLISH)
            throw new ValidationException("Chỉ khóa học ở trạng thái Draft hoặc Unpublish mới có thể xóa mềm.");

        course.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PaginationResult<IEnumerable<CourseResponseDTO>>> GetAllCoursesAsync(int pageIndex, int pageSize, string? search = null, CourseStatus? status = null)
    {
        Expression<Func<Course, bool>>? filter = null;

        if (status.HasValue && !string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            var st = status.Value;
            filter = c => c.Status == st
                && c.CurrentVersion != null
                && ((c.CurrentVersion.TitleEN != null && c.CurrentVersion.TitleEN.Contains(s))
                    || (c.CurrentVersion.TitleVN != null && c.CurrentVersion.TitleVN.Contains(s)));
        }
        else if (status.HasValue)
        {
            var st = status.Value;
            filter = c => c.Status == st;
        }
        else if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            filter = c => c.CurrentVersion != null
                && ((c.CurrentVersion.TitleEN != null && c.CurrentVersion.TitleEN.Contains(s))
                    || (c.CurrentVersion.TitleVN != null && c.CurrentVersion.TitleVN.Contains(s)));
        }

        var result = await _unitOfWork.Courses
            .GetAllWithCurrentVersionAsync(
                filter,
                query => query.OrderByDescending(c => c.CreateAt),
                pageIndex,
                pageSize);

        var entities = result.Data.ToList();
        var mapped = entities.Select(c => _mapper.Map<CourseResponseDTO>(c)).ToList();

        var userCache = await BuildUserLookupAsync(entities);
        PopulateMappedCoursesUsers(entities, mapped, userCache);

        return new PaginationResult<IEnumerable<CourseResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<CourseResponseDTO> GetCourseByIdAsync(Guid courseId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithCurrentVersionAsync(courseId)
            ?? throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        var courseResponse = _mapper.Map<CourseResponseDTO>(course);
        await PopulateCreatorAsync(courseResponse, course.CreateBy);
        await PopulateCurrentVersionUpdaterAsync(courseResponse, course.CurrentVersion?.UpdateBy);

        return courseResponse;
    }

    public async Task PublishCourseAsync(Guid courseId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        var currentVersion = course.CourseVersions.FirstOrDefault(v => v.CourseVersionID == course.CurrentVersionID);
        if (currentVersion == null)
            throw new ValidationException("Không thể publish khóa học khi chưa có phiên bản hiện tại.");

        if (currentVersion.Status != CourseVersionStatus.ACTIVE)
            throw new ValidationException("Chỉ có thể publish khóa học khi phiên bản hiện tại đang ở trạng thái Active.");

        course.Publish();

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnpublishCourseAsync(Guid courseId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        var currentVersion = course.CourseVersions.FirstOrDefault(v => v.CourseVersionID == course.CurrentVersionID);
        if (currentVersion != null)
        {
            if (currentVersion.Status == CourseVersionStatus.INACTIVE)
                throw new ValidationException("Phiên bản hiện tại đã xóa mềm, chỉ được xem.");

            if (currentVersion.Status == CourseVersionStatus.ACTIVE)
                currentVersion.Deprecate(_currentUser.UserId, _clock.Now);
        }

        course.Unpublish();

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<CourseResponseDTO>> GetCoursesByIdsAsync(IEnumerable<Guid> courseIds)
    {
        var ids = courseIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0)
            return [];

        var result = await _unitOfWork.Courses.GetAllWithCurrentVersionAsync(
            filter: c => ids.Contains(c.CourseID),
            pageIndex: 1,
            pageSize: ids.Count);

        var entities = result.Data.ToList();
        var data = entities
            .Select(c => _mapper.Map<CourseResponseDTO>(c))
            .ToList();

        var userCache = await BuildUserLookupAsync(entities);
        PopulateMappedCoursesUsers(entities, data, userCache);

        return data
            .OrderBy(c => ids.IndexOf(c.CourseID))
            .ToList();
    }

    private async Task PopulateCreatorAsync(CourseResponseDTO course, Guid createBy)
    {
        course.Creator = await _userDisplayNameService.ResolveUserDisplayNameAsync(createBy);
    }

    private async Task PopulateCreatorAsync(CourseDetailResponseDTO course, Guid createBy)
    {
        course.Creator = await _userDisplayNameService.ResolveUserDisplayNameAsync(createBy);
    }

    private async Task PopulateCurrentVersionUpdaterAsync(CourseResponseDTO course, Guid? updateBy)
    {
        if (!updateBy.HasValue || updateBy.Value == Guid.Empty)
            return;

        course.CurrentVersion!.Updater = await _userDisplayNameService.ResolveUserDisplayNameAsync(updateBy.Value);
    }

    private async Task<Dictionary<Guid, SimpleUserReponse?>> BuildUserLookupAsync(IEnumerable<Course> courses)
    {
        var userIds = courses
            .SelectMany(c => new Guid?[] { c.CreateBy, c.CurrentVersion?.UpdateBy })
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var lookup = await _userDisplayNameService.ResolveUsersDisplayNameAsync(userIds);
        return lookup.ToDictionary(x => x.Key, x => x.Value);
    }

    private static void PopulateMappedCoursesUsers(
        IEnumerable<Course> entities,
        IEnumerable<CourseResponseDTO> dtos,
        IReadOnlyDictionary<Guid, SimpleUserReponse?> userLookup)
    {
        foreach (var (entity, dto) in entities.Zip(dtos))
        {
            if (userLookup.TryGetValue(entity.CreateBy, out var creator))
            {
                dto.Creator = creator;
            }

            var updaterId = entity.CurrentVersion?.UpdateBy;
            if (updaterId.HasValue && updaterId.Value != Guid.Empty && dto.CurrentVersion != null
                && userLookup.TryGetValue(updaterId.Value, out var updater))
            {
                dto.CurrentVersion.Updater = updater;
            }
        }
    }
}
