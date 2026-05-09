using AutoMapper;
using Droniverse.Academy.Application.Common.Extensions;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.Enums;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.Validators;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Domain.Models;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;
using Droniverse.Shared.Services.IServices;
using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Services;


public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IMapper _mapper;
    private readonly IUserLookupService _userLookupService;
    private readonly CommunityMicroserviceClient _communityMicroserviceClient;
    public CourseService(IUnitOfWork unitOfWork, IClock clock, ICurrentUserService current, IMapper mapper, IUserLookupService userLookupService, CommunityMicroserviceClient communityMicroserviceClient)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUser = current;
        _mapper = mapper;
        _userLookupService = userLookupService;
        _communityMicroserviceClient = communityMicroserviceClient;
    }

    public async Task<CourseDetailResponseDTO> CreateCourseAsync(CreateCourseRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Version == null)
            throw new ValidationException("Thông tin phiên bản khóa học là bắt buộc.");

        CourseVersionValidator.ValidateCreateData(request.Version);

        var level = await _unitOfWork.Levels.GetByIdAsync(request.LevelID);
        if (level == null)
            throw new BaseException("Không tìm thấy level.", "NOT_FOUND");

        var course = new Course
        {
            CourseID = Guid.NewGuid(),
            LevelID = level.LevelID,
            DroneID = level.DroneID
        };
        course.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Courses.AddAsync(course);
        await _unitOfWork.SaveChangesAsync();

        var version = _mapper.Map<CourseVersion>(request.Version);
        version.CourseVersionID = Guid.NewGuid();
        version.CourseID = course.CourseID;
        version.Version = 1;
        version.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.CourseVersions.AddAsync(version);
        await _unitOfWork.SaveChangesAsync();

        course.CurrentVersionID = version.CourseVersionID;
        await _unitOfWork.SaveChangesAsync();

        var createdCourse = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(course.CourseID)
            ?? course;

        var response = _mapper.Map<CourseDetailResponseDTO>(createdCourse);
        response.Creator = await ResolveUserAsync(createdCourse.CreateBy);
        response.PrerequisiteCourses = [];

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

    public async Task<PaginationResult<IEnumerable<CourseResponseDTO>>> GetAllCoursesAsync(
        int pageIndex,
        int pageSize,
        string? search = null,
        CourseStatus? status = null,
        Guid? droneId = null,
        Guid? levelId = null)
    {
        Expression<Func<Course, bool>> filter = c => true;

        if (status.HasValue)
        {
            var st = status.Value;
            filter = filter.And(c => c.Status == st);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            filter = filter.And(c => c.CurrentVersion != null
                && ((c.CurrentVersion.TitleEN != null && c.CurrentVersion.TitleEN.Contains(s))
                    || (c.CurrentVersion.TitleVN != null && c.CurrentVersion.TitleVN.Contains(s))));
        }

        if (droneId.HasValue && droneId.Value != Guid.Empty)
        {
            var filterDroneId = droneId.Value;
            filter = filter.And(c => c.DroneID == filterDroneId);
        }

        if (levelId.HasValue && levelId.Value != Guid.Empty)
        {
            var filterLevelId = levelId.Value;
            filter = filter.And(c => c.LevelID == filterLevelId);
        }

        var result = await _unitOfWork.Courses
            .GetAllWithCurrentVersionAsync(
                filter,
                query => query.OrderBy(c => c.Level!.LevelNumber)
                              .ThenByDescending(c => c.CreateAt),
                pageIndex,
                pageSize);

        var entities = result.Data.ToList();
        var mapped = entities.Select(c => _mapper.Map<CourseResponseDTO>(c)).ToList();
        var prerequisiteLookup = await BuildPrerequisiteLookupAsync(entities.Select(x => x.CourseID));

        var referenceIds = entities
            .Select(c => c.CourseID)
            .Distinct()
            .ToList();

        var products = await _communityMicroserviceClient.GetProductsBulkByReferenceIdsAsync(referenceIds);
        var productsByReferenceId = products
            .Where(p => p.ReferenceId != Guid.Empty)
            .GroupBy(p => p.ReferenceId)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var (entity, dto) in entities.Zip(mapped))
        {
            var referenceId = entity.CourseID;
            if (productsByReferenceId.TryGetValue(referenceId, out var miniProduct))
            {
                dto.MiniProduct = _mapper.Map<ProductMiniResponseDTO>(miniProduct);
            }

            dto.PrerequisiteCourses = prerequisiteLookup.TryGetValue(referenceId, out var prerequisiteCourses)
                ? prerequisiteCourses
                : [];
        }

        var userCache = await BuildUserLookupAsync(entities);
        mapped = MapCoursesUsers(entities, mapped, userCache);

        return new PaginationResult<IEnumerable<CourseResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<CourseResponseDTO> GetCourseByIdAsync(Guid courseId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithCurrentVersionAsync(courseId)
            ?? throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        var courseResponse = _mapper.Map<CourseResponseDTO>(course);
        var prerequisiteLookup = await BuildPrerequisiteLookupAsync([course.CourseID]);
        courseResponse.PrerequisiteCourses = prerequisiteLookup.TryGetValue(course.CourseID, out var prerequisiteCourses)
            ? prerequisiteCourses
            : [];
        courseResponse.Creator = await ResolveUserAsync(course.CreateBy);
        if (courseResponse.CurrentVersion != null)
        {
            courseResponse.CurrentVersion.Updater = await ResolveUserAsync(course.CurrentVersion?.UpdateBy);
        }

        if (course.CurrentVersion != null)
        {
            var product = await _communityMicroserviceClient.GetProductByReferenceIdAsync(course.CourseID);
            courseResponse.MiniProduct = product == null
                ? null
                : _mapper.Map<ProductMiniResponseDTO>(product);
        }

        return courseResponse;
    }

    public async Task<CourseOverviewResponseDTO> GetCourseOverviewAsync(
        Guid clubId,
        Guid courseId,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUser.UserId;
        var course = await GetCourseWithCurrentVersionAsync(courseId);
        var courseVersionId = EnsureCurrentVersionId(course, courseId);
        var overviewData = await GetCourseOverviewDataAsync(courseVersionId, currentUserId, cancellationToken);

        var response = MapOverviewResponse(overviewData);
        await PopulatePrerequisitesAndEligibilityAsync(response, overviewData.CourseID, course, currentUserId);
        response.IsPrerequisitesCompleted = await ArePrerequisitesCompletedAsync(response.PrerequisiteCourses, currentUserId);
        response.EnrollmentID = await GetEnrollmentIdAsync(clubId, currentUserId, courseVersionId);
        await PopulateUserInfoAsync(response, overviewData);
        response.LastUpdatedAt = overviewData.LastUpdatedAt;
        await PopulateExternalDataAsync(response, overviewData.CourseID, clubId, cancellationToken);

        return response;
    }

    private async Task<Course> GetCourseWithCurrentVersionAsync(Guid courseId)
    {
        return await _unitOfWork.Courses.GetByIdWithCurrentVersionAsync(courseId)
            ?? throw new NotFoundException($"Không tìm thấy khóa học với id {courseId}.");
    }

    private static Guid EnsureCurrentVersionId(Course course, Guid courseId)
    {
        return course.CurrentVersionID
            ?? throw new NotFoundException($"Khóa học {courseId} chưa có phiên bản hiện tại.");
    }

    private async Task<CourseOverviewData> GetCourseOverviewDataAsync(
        Guid courseVersionId,
        Guid currentUserId,
        CancellationToken cancellationToken)
    {
        var overviewData = await _unitOfWork.CourseVersions
            .GetCourseOverviewDataAsync(courseVersionId, currentUserId, cancellationToken);

        if (overviewData == null)
            throw new NotFoundException($"Không tìm thấy phiên bản khóa học với id {courseVersionId}.");

        return overviewData;
    }

    private CourseOverviewResponseDTO MapOverviewResponse(CourseOverviewData overviewData)
    {
        return _mapper.Map<CourseOverviewResponseDTO>(overviewData);
    }

    private async Task PopulatePrerequisitesAndEligibilityAsync(
        CourseOverviewResponseDTO response,
        Guid courseId,
        Course course,
        Guid currentUserId)
    {
        var prerequisiteLookup = await BuildPrerequisiteLookupAsync([courseId]);
        response.PrerequisiteCourses = prerequisiteLookup.TryGetValue(courseId, out var prerequisiteCourses)
            ? prerequisiteCourses
            : [];

        response.Level = _mapper.Map<LevelMiniResponse?>(course.Level);
        response.Drone = _mapper.Map<DroneMiniResponse?>(course.Drone);
        response.IsEligibleByLevel = await CanCurrentUserLearnCourseByLevelAsync(course, currentUserId);
    }

    private async Task<bool> ArePrerequisitesCompletedAsync(
        IReadOnlyCollection<PrerequisiteCourseMiniReponse> prerequisiteCourses,
        Guid currentUserId)
    {
        var prerequisiteCourseIds = prerequisiteCourses
            .Select(x => x.CourseID)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (prerequisiteCourseIds.Count == 0)
        {
            return true;
        }

        var completedEnrollments = await _unitOfWork.Enrollments.GetAllAsync(
            filter: x => x.UserID == currentUserId
                         && x.Status == EnrollStatus.COMPLETED
                         && prerequisiteCourseIds.Contains(x.CourseID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var completedCourseIds = completedEnrollments.Data
            .Select(x => x.CourseID)
            .Distinct()
            .ToHashSet();

        return prerequisiteCourseIds.All(completedCourseIds.Contains);
    }

    private async Task<Guid?> GetEnrollmentIdAsync(Guid clubId, Guid currentUserId, Guid courseVersionId)
    {
        var enrollment = await _unitOfWork.Enrollments.GetByConditionAsync(
            x => x.UserID == currentUserId
                 && x.ClubID == clubId
                 && x.CourseVersionID == courseVersionId);

        return enrollment?.EnrollmentID;
    }

    private async Task PopulateUserInfoAsync(CourseOverviewResponseDTO response, CourseOverviewData overviewData)
    {
        var userIds = new List<Guid> { overviewData.AuthorId };
        if (overviewData.LastUpdatedById.HasValue && overviewData.LastUpdatedById.Value != Guid.Empty)
        {
            userIds.Add(overviewData.LastUpdatedById.Value);
        }

        var userLookup = await _userLookupService.BuildUserLookupAsync(userIds.Distinct());

        if (userLookup.TryGetValue(overviewData.AuthorId, out var author))
        {
            response.Author = author;
        }

        if (overviewData.LastUpdatedById.HasValue
            && userLookup.TryGetValue(overviewData.LastUpdatedById.Value, out var lastUpdatedBy))
        {
            response.LastUpdatedBy = lastUpdatedBy;
        }
    }

    private async Task PopulateExternalDataAsync(
        CourseOverviewResponseDTO response,
        Guid courseId,
        Guid clubId,
        CancellationToken cancellationToken)
    {
        var product = await _communityMicroserviceClient
            .GetProductByReferenceIdAsync(courseId, cancellationToken);
        response.MiniProduct = product == null
            ? null
            : _mapper.Map<ProductMiniResponseDTO>(product);

        response.ClubCourseOwn = await _communityMicroserviceClient
            .GetRemainingQuantityAsync(clubId, courseId, cancellationToken);
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

    public async Task<PagedCourseBulkResponse> GetCoursesClub(
        Guid clubId,
        CourseBulkSearchRequest searchRequest)
    {
        searchRequest ??= new CourseBulkSearchRequest();
        var droneId = await _communityMicroserviceClient.GetDroneFromClubAsync(clubId);

        var pageIndex = searchRequest.CurrentPage < 1 ? 1 : searchRequest.CurrentPage;
        var pageSize = searchRequest.PageSize < 1 ? 5 : searchRequest.PageSize;
        var normalizedCourseName = searchRequest.CourseName?.Trim();

        if (droneId == Guid.Empty)
        {
            return new PagedCourseBulkResponse
            {
                TotalItems = 0,
                Items = []
            };
        }

        Expression<Func<Course, bool>> filter = c =>
            c.CurrentVersion != null &&
            c.Status == CourseStatus.PUBLISH &&
            c.DroneID == droneId &&

            // Filter theo Level
            (!searchRequest.LevelId.HasValue ||
             searchRequest.LevelId.Value == Guid.Empty ||
             c.LevelID == searchRequest.LevelId.Value) &&

            // Filter theo tên
            (
                string.IsNullOrWhiteSpace(normalizedCourseName) ||
                (c.CurrentVersion.TitleEN != null && c.CurrentVersion.TitleEN.Contains(normalizedCourseName)) ||
                (c.CurrentVersion.TitleVN != null && c.CurrentVersion.TitleVN.Contains(normalizedCourseName))
            );

        var courseResult = await _unitOfWork.Courses.GetAllWithCurrentVersionAsync(
            filter: filter,
            orderBy: q => q.OrderBy(c => c.CourseID),
            pageIndex: pageIndex,
            pageSize: pageSize);

        var courses = courseResult.Data.ToList();

        if (courses.Count == 0)
        {
            return new PagedCourseBulkResponse
            {
                TotalItems = courseResult.TotalRecords,
                Items = []
            };
        }

        // Lấy list versionId
        var courseVersionIds = courses
            .Select(c => c.CurrentVersion!.CourseVersionID)
            .Distinct()
            .ToList();

        var courseIds = courses
            .Select(c => c.CourseID)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        // Lấy thống kê
        var participantCountByVersionId = await _unitOfWork.Enrollments
            .GetActiveOrCompletedParticipantCountsByCourseVersionIdsAsync(courseVersionIds);

        var ratingByVersionId = await _unitOfWork.Feedbacks
            .GetAverageRatingsByCourseVersionIdsAsync(courseVersionIds);

        var productByCourseId = (await _communityMicroserviceClient.GetProductsBulkByReferenceIdsAsync(courseIds))
            .Where(p => p.ReferenceId != Guid.Empty)
            .GroupBy(p => p.ReferenceId)
            .ToDictionary(g => g.Key, g => g.First().Price);

        // Map DTO
        var data = courses.Select(c =>
        {
            var currentVersion = c.CurrentVersion!;
            var versionId = currentVersion.CourseVersionID;

            participantCountByVersionId.TryGetValue(versionId, out var numberOfParticipants);
            ratingByVersionId.TryGetValue(versionId, out var rating);

            return new CourseBulkResponseDTO
            {
                CourseId = c.CourseID,
                CourseVersionId = versionId,
                TitleVN = currentVersion.TitleVN,
                TitleEN = currentVersion.TitleEN,

                Level = c.Level == null ? null : new CourseLevelMiniResponseDTO
                {
                    LevelID = c.Level.LevelID,
                    LevelNumber = c.Level.LevelNumber,
                    Name = c.Level.Name
                },

                Drone = c.Drone == null ? null : new CourseDroneMiniResponseDTO
                {
                    DroneID = c.Drone.DroneID,
                    Name = c.Drone.DroneNameEN,
                    ImgURL = c.Drone.ImgURL
                },

                EstimatedDuration = currentVersion.EstimatedDuration,
                Price = productByCourseId.TryGetValue(c.CourseID, out var price) ? price : null,
                Rating = rating,
                NumberOfParticipants = numberOfParticipants,
                ImageUrl = currentVersion.ImageUrl
            };
        }).ToList();

        // Sort
        var orderedData = searchRequest.ParticipationSort switch
        {
            CourseParticipationSort.MostPopular =>
                data.OrderByDescending(x => x.NumberOfParticipants)
                    .ThenBy(x => x.CourseId),

            CourseParticipationSort.LeastPopular =>
                data.OrderBy(x => x.NumberOfParticipants)
                    .ThenBy(x => x.CourseId),

            _ => data.OrderBy(x => x.CourseId)
        };

        return new PagedCourseBulkResponse
        {
            TotalItems = courseResult.TotalRecords,
            Items = orderedData.ToList()
        };
    }

    public async Task<PagedCourseBulkResponse> GetHotCoursesByIdsAsync(
        Guid clubId,
        HotCoursesSearchRequest searchRequest)
    {
        var userRoles = _currentUser.Roles;
        var isMember = userRoles.Contains(Roles.ClubMember);
        if (isMember)
        {
            bool isActiveParticipant = await _communityMicroserviceClient.CheckParticipantByClubAsync(clubId, _currentUser.UserId);
            if (!isActiveParticipant)
            {
                throw new ForbiddenException("Bạn đã rời câu lạc bộ này, vui lòng liên hệ quản lý câu lạc bộ (club manager) hoặc admin để biết thêm chi tiết.");
            }
        }

        searchRequest ??= new HotCoursesSearchRequest();
        var droneId = await _communityMicroserviceClient.GetDroneFromClubAsync(clubId);

        var pageIndex = searchRequest.CurrentPage < 1 ? 1 : searchRequest.CurrentPage;
        const int pageSize = 4;

        if (droneId == Guid.Empty)
        {
            return new PagedCourseBulkResponse
            {
                TotalItems = 0,
                Items = []
            };
        }

        Expression<Func<Course, bool>> filter = c =>
            c.CurrentVersion != null &&
            c.Status == CourseStatus.PUBLISH &&
            c.DroneID == droneId &&
            (!searchRequest.LevelId.HasValue || searchRequest.LevelId.Value == Guid.Empty || c.LevelID == searchRequest.LevelId.Value);

        var filteredCourses = await _unitOfWork.Courses.GetAllWithCurrentVersionNoPagingAsync(filter: filter);
        var ids = filteredCourses
            .Select(c => c.CourseID)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return new PagedCourseBulkResponse
            {
                TotalItems = 0,
                Items = []
            };
        }

        var courseResult = await _unitOfWork.Courses.GetHotCoursesByIdsWithCurrentVersionAsync(
            courseIds: ids,
            currentUserId: _currentUser.UserId,
            ownedOnly: false,
            courseName: null,
            pageIndex: pageIndex,
            pageSize: pageSize);

        var items = courseResult.Data.ToList();
        if (items.Count == 0)
        {
            return new PagedCourseBulkResponse
            {
                TotalItems = courseResult.TotalRecords,
                Items = []
            };
        }

        var itemCourseIds = items
            .Select(x => x.CourseId)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        var productTask = _communityMicroserviceClient
            .GetProductsBulkByReferenceIdsAsync(itemCourseIds);



        await Task.WhenAll(productTask);

        var productByCourseId = productTask.Result
            .Where(p => p.ReferenceId != Guid.Empty)
            .ToDictionary(p => p.ReferenceId, p => p.Price);



        foreach (var item in items)
        {
            item.Price = productByCourseId.TryGetValue(item.CourseId, out var price)
                ? price
                : 0m;
        }

        return new PagedCourseBulkResponse
        {
            TotalItems = courseResult.TotalRecords,
            Items = items
        };
    }

    public async Task<IEnumerable<SimpleCourseResponse>> GetCoursesByIdsSimpleAsync(Guid clubId)
    {

        var userRoles = _currentUser.Roles;
        var isMember = userRoles.Contains(Roles.ClubMember);
        if (isMember)
        {
            bool isActiveParticipant = await _communityMicroserviceClient.CheckParticipantByClubAsync(clubId, _currentUser.UserId);
            if (!isActiveParticipant)
            {
                throw new ForbiddenException("Bạn đã rời câu lạc bộ này, vui lòng liên hệ quản lý câu lạc bộ (club manager) hoặc admin để biết thêm chi tiết.");
            }
        }

        var droneId = await _communityMicroserviceClient.GetDroneFromClubAsync(clubId);

        if (droneId == Guid.Empty)
        {
            return [];
        }

        Expression<Func<Course, bool>> filter = c =>
            c.CurrentVersion != null &&
            c.Status == CourseStatus.PUBLISH &&
            c.DroneID == droneId;

        var coursesByDrone = await _unitOfWork.Courses.GetAllWithCurrentVersionNoPagingAsync(filter);
        var ids = coursesByDrone
            .Select(c => c.CourseID)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        var courses = (await _unitOfWork.Courses.GetSimpleCoursesByIdsAsync(ids)).ToList();
        if (courses.Count == 0)
        {
            return [];
        }

        var orderLookup = ids
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);

        return courses
            .OrderBy(c => orderLookup.GetValueOrDefault(c.CourseId, int.MaxValue))
            .ToList();
    }

    private async Task<SimpleUserReponse?> ResolveUserAsync(Guid? userId)
    {
        if (!userId.HasValue || userId.Value == Guid.Empty)
            return null;

        var userLookup = await _userLookupService.BuildUserLookupAsync(new[] { userId.Value });
        userLookup.TryGetValue(userId.Value, out var user);

        return user;
    }

    private async Task<Dictionary<Guid, SimpleUserReponse?>> BuildUserLookupAsync(IEnumerable<Course> courses)
    {
        var userIds = courses
            .SelectMany(c => new Guid?[] { c.CreateBy, c.CurrentVersion?.UpdateBy })
            .ToDistinctValidIds();

        return await _userLookupService.BuildUserLookupAsync(userIds);
    }

    private async Task<Dictionary<Guid, List<PrerequisiteCourseMiniReponse>>> BuildPrerequisiteLookupAsync(
        IEnumerable<Guid> courseIds)
    {
        var ids = courseIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return [];
        }

        var prerequisites = await _unitOfWork.PrerequisiteCourses
            .GetByCourseIdsWithRequiredCourseAsync(ids);

        //ToDo: mở lại filter khi đã có data thực tế, hiện tại dữ liệu test chưa đầy đủ nên tạm thời bỏ filter để test end-to-end dễ dàng hơn
        // STEP 1: Filter + Group
        var filteredGroups = prerequisites
            //.Where(x =>
            //    x.RequiredCourse != null
            //    && x.RequiredCourse.Status == CourseStatus.PUBLISH
            //    && x.RequiredCourse.CurrentVersion != null
            //    && x.RequiredCourse.CurrentVersion.Status == CourseVersionStatus.ACTIVE)
            .Where(x => x.RequiredCourse != null)
            .GroupBy(x => x.CourseID)
            .ToList(); // materialize để debug


        // STEP 2: Map sang Dictionary
        var result = filteredGroups.ToDictionary(
            g => g.Key,
            g => g
                .Select(x => new PrerequisiteCourseMiniReponse
                {
                    CourseID = x.PrerequisiteCourseID,
                    Level = x.RequiredCourse!.Level == null ? null : _mapper.Map<LevelMiniResponse>(x.RequiredCourse.Level),
                    ImageUrl = x.RequiredCourse.CurrentVersion?.ImageUrl ?? string.Empty,
                    TitleVN = x.RequiredCourse!.CurrentVersion!.TitleVN,
                    TitleEN = x.RequiredCourse.CurrentVersion.TitleEN
                })
                .DistinctBy(x => x.CourseID)
                .ToList()
        );

        return result;
    }

    private static List<CourseResponseDTO> MapCoursesUsers(
        IEnumerable<Course> entities,
        List<CourseResponseDTO> dtos,
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

        return dtos;
    }

    private async Task<bool> CanCurrentUserLearnCourseByLevelAsync(Course course, Guid userId)
    {
        if (course.Level == null)
            return false;

        var userLevelsResult = await _unitOfWork.UserLevels.GetAllAsync(
            filter: x => x.UserID == userId && x.Level.DroneID == course.Level.DroneID,
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "Level");

        var maxLevelNumber = userLevelsResult.Data
            .Select(x => x.Level.LevelNumber)
            .DefaultIfEmpty(0)
            .Max();

        if (maxLevelNumber == 0 && course.Level.LevelNumber == 1)
            return true;

        return maxLevelNumber >= course.Level.LevelNumber;
    }

    public async Task<PagedManagerCoursesBulkResponse> GetCoursesByIdsManagementAsync(
        Guid clubId,
        ManagerCourseBulkSearchRequest searchRequest)
    {
        searchRequest ??= new ManagerCourseBulkSearchRequest();
        var droneId = await _communityMicroserviceClient.GetDroneFromClubAsync(clubId);

        var pageIndex = searchRequest.CurrentPage < 1 ? 1 : searchRequest.CurrentPage;
        var pageSize = searchRequest.PageSize < 1 ? 5 : searchRequest.PageSize;

        if (droneId == Guid.Empty)
        {
            return CreateEmptyManagerCoursesPagedResponse(pageIndex, pageSize);
        }

        Expression<Func<Course, bool>> filter = c =>
            c.CurrentVersion != null &&
            c.DroneID == droneId &&
            (!searchRequest.LevelId.HasValue || searchRequest.LevelId.Value == Guid.Empty || c.LevelID == searchRequest.LevelId.Value);

        var orderBy = BuildManagerCoursesOrderBy(searchRequest);

        var coursePage = await _unitOfWork.Courses.GetAllWithCurrentVersionAsync(
            filter: filter,
            orderBy: orderBy,
            pageIndex: pageIndex,
            pageSize: pageSize);

        var courses = coursePage.Data.ToList();
        if (courses.Count == 0)
        {
            return new PagedManagerCoursesBulkResponse
            {
                TotalItems = coursePage.TotalRecords,
                Items = []
            };
        }

        var versionIds = courses
            .Where(c => c.CurrentVersion != null)
            .Select(c => c.CurrentVersion!.CourseVersionID)
            .Distinct()
            .ToList();

        var participantByVersionId = await _unitOfWork.Enrollments
            .GetActiveOrCompletedParticipantCountsByCourseVersionIdsAsync(versionIds);

        var courseIds = courses
            .Select(c => c.CourseID)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        var productByCourseId = (await _communityMicroserviceClient.GetProductsBulkByReferenceIdsAsync(courseIds))
            .Where(p => p.ReferenceId != Guid.Empty)
            .GroupBy(p => p.ReferenceId)
            .ToDictionary(g => g.Key, g => g.First().Price);

        var items = courses
            .Where(c => c.CurrentVersion != null)
            .Select(c =>
            {
                var currentVersion = c.CurrentVersion!;
                participantByVersionId.TryGetValue(currentVersion.CourseVersionID, out var participants);

                return new ManagerCoursesBulkResponseDTO
                {
                    CourseId = c.CourseID,
                    CourseVersionId = currentVersion.CourseVersionID,
                    TitleVN = currentVersion.TitleVN,
                    TitleEN = currentVersion.TitleEN,
                    ImageUrl = currentVersion.ImageUrl ?? string.Empty,
                    NumberOfParticipants = participants,
                    EstimatedDuration = currentVersion.EstimatedDuration,
                    Price = productByCourseId.TryGetValue(c.CourseID, out var price) ? price : null
                };
            })
            .ToList();

        return new PagedManagerCoursesBulkResponse
        {
            TotalItems = coursePage.TotalRecords,
            Items = items
        };
    }

    private static PagedManagerCoursesBulkResponse CreateEmptyManagerCoursesPagedResponse(int pageIndex, int pageSize)
    {
        return new PagedManagerCoursesBulkResponse
        {
            TotalItems = 0,
            Items = []
        };
    }

    private static bool MatchProfitType(decimal? price, ClubCourseProfit? expectedProfitType)
    {
        if (!expectedProfitType.HasValue)
        {
            return true;
        }

        var actualProfitType = price.GetValueOrDefault() > 0
            ? ClubCourseProfit.PROFIT
            : ClubCourseProfit.NONPROFIT;

        return actualProfitType == expectedProfitType.Value;
    }

    private static Func<IQueryable<Course>, IOrderedQueryable<Course>>? BuildManagerCoursesOrderBy(
       ManagerCourseBulkSearchRequest searchRequest)
    {
        var sortBy = searchRequest.CourseSortBy ?? ManagerCourseSortBy.Participants_Quantity;
        var sortDirection = searchRequest.CourseSortDirection ?? SortDirection.Asc;

        if (sortBy != ManagerCourseSortBy.Participants_Quantity)
            return null;

        return q =>
        {
            var query = sortDirection == SortDirection.Desc
                ? q.OrderByDescending(c =>
                    c.CurrentVersion!.Enrollments.Count(e =>
                        e.Status == EnrollStatus.ACTIVE ||
                        e.Status == EnrollStatus.COMPLETED))
                : q.OrderBy(c =>
                    c.CurrentVersion!.Enrollments.Count(e =>
                        e.Status == EnrollStatus.ACTIVE ||
                        e.Status == EnrollStatus.COMPLETED));

            return query.ThenBy(c => c.CourseID);
        };
    }

    public async Task<IEnumerable<CourseStatisticInterServiceDto>> GetAllCoursesWithStatisticsAsync()
    {
        var courses = await _unitOfWork.Courses.GetAllWithCurrentVersionNoPagingAsync(c => true);
        var coursesList = courses.ToList();

        if (coursesList.Count == 0)
        {
            return [];
        }

        var versionIds = coursesList
            .Where(c => c.CurrentVersion != null)
            .Select(c => c.CurrentVersion!.CourseVersionID)
            .Distinct()
            .ToList();

        var participantByVersionId = await _unitOfWork.Enrollments
            .GetActiveOrCompletedParticipantCountsByCourseVersionIdsAsync(versionIds);

        var ratingByVersionId = await _unitOfWork.Feedbacks
            .GetAverageRatingsByCourseVersionIdsAsync(versionIds);

        return coursesList.Select(c =>
        {
            var currentVersionId = c.CurrentVersion?.CourseVersionID;
            participantByVersionId.TryGetValue(currentVersionId ?? Guid.Empty, out var participants);
            ratingByVersionId.TryGetValue(currentVersionId ?? Guid.Empty, out var rating);

            return new CourseStatisticInterServiceDto
            {
                CourseId = c.CourseID,
                CurrentVersionId = currentVersionId,
                TitleVN = c.CurrentVersion?.TitleVN ?? string.Empty,
                TitleEN = c.CurrentVersion?.TitleEN ?? string.Empty,
                ImageUrl = c.CurrentVersion?.ImageUrl,
                IsPublished = c.Status == CourseStatus.PUBLISH,
                TotalLearners = participants,
                AverageRating = rating
            };
        });
    }
}
