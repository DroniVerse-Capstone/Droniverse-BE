using AutoMapper;
using Droniverse.Academy.Application.Common.Extensions;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.Enums;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService;
using Droniverse.Community.Domain.Enums;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Request;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;
using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Services;


public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IMapper _mapper;
    private readonly IUserDisplayNameService _userDisplayNameService;
    private readonly CommunityMicroserviceClient _communityMicroserviceClient;
    public CourseService(IUnitOfWork unitOfWork, IClock clock, ICurrentUserService current, IMapper mapper, IUserDisplayNameService userDisplayNameService, CommunityMicroserviceClient communityMicroserviceClient)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUser = current;
        _mapper = mapper;
        _userDisplayNameService = userDisplayNameService;
        _communityMicroserviceClient = communityMicroserviceClient;
    }

    public async Task<CourseDetailResponseDTO> CreateCourseAsync()
    {
        var course = new Course
        {
            CourseID = Guid.NewGuid(),
            CourseVersions = []
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

        var result = await _unitOfWork.Courses
            .GetAllWithCurrentVersionAsync(
                filter,
                query => query.OrderByDescending(c => c.CreateAt),
                pageIndex,
                pageSize);

        var entities = result.Data.ToList();
        var mapped = entities.Select(c => _mapper.Map<CourseResponseDTO>(c)).ToList();

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
        }

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
        Guid courseVersionId,
        CancellationToken cancellationToken = default)
    {
        var overviewData = await _unitOfWork.CourseVersions
            .GetCourseOverviewDataAsync(courseVersionId, _currentUser.UserId, cancellationToken);

        if (overviewData == null)
            throw new NotFoundException($"Không tìm thấy phiên bản khóa học với id {courseVersionId}.");

        var response = _mapper.Map<CourseOverviewResponseDTO>(overviewData);

        var userIds = new List<Guid> { overviewData.AuthorId };
        if (overviewData.LastUpdatedById.HasValue && overviewData.LastUpdatedById.Value != Guid.Empty)
        {
            userIds.Add(overviewData.LastUpdatedById.Value);
        }

        var userLookup = await _userDisplayNameService.ResolveUsersDisplayNameAsync(userIds.Distinct());

        if (userLookup.TryGetValue(overviewData.AuthorId, out var author))
        {
            response.Author = author;
        }

        if (overviewData.LastUpdatedById.HasValue
            && userLookup.TryGetValue(overviewData.LastUpdatedById.Value, out var lastUpdatedBy))
        {
            response.LastUpdatedBy = lastUpdatedBy;
        }

        response.LastUpdatedAt = overviewData.LastUpdatedAt;
        var product = await _communityMicroserviceClient.GetProductByReferenceIdAsync(overviewData.CourseID, cancellationToken);
        response.MiniProduct = product == null
            ? null
            : _mapper.Map<ProductMiniResponseDTO>(product);
        return response;
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

    public async Task<PagedCourseBulkResponse> GetCoursesByIdsAsync(
    CourseBulkSearchRequest searchRequest,
    IEnumerable<Guid> courseIds)
    {
        searchRequest ??= new CourseBulkSearchRequest();

        var pageIndex = searchRequest.CurrentPage < 1 ? 1 : searchRequest.CurrentPage;
        var pageSize = searchRequest.PageSize < 1 ? 5 : searchRequest.PageSize;
        var normalizedCourseName = searchRequest.CourseName?.Trim();

        var ids = courseIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (searchRequest.CourseOwner == CourseOwnerFilter.Owned && ids.Count == 0)
            return new PagedCourseBulkResponse
            {
                TotalItems = 0,
                Items = []
            };

        Expression<Func<Course, bool>> filter = c =>
            c.CurrentVersion != null &&
            c.Status == CourseStatus.PUBLISH &&
            (!searchRequest.Level.HasValue || c.CurrentVersion.Level == searchRequest.Level.Value) &&
            (
                string.IsNullOrWhiteSpace(normalizedCourseName) ||
                (c.CurrentVersion.TitleEN != null && c.CurrentVersion.TitleEN.Contains(normalizedCourseName)) ||
                (c.CurrentVersion.TitleVN != null && c.CurrentVersion.TitleVN.Contains(normalizedCourseName))
            ) &&
            (
                searchRequest.CourseOwner == CourseOwnerFilter.All ||
                (searchRequest.CourseOwner == CourseOwnerFilter.Owned && ids.Contains(c.CourseID)) ||
                (searchRequest.CourseOwner == CourseOwnerFilter.NotOwned && !ids.Contains(c.CourseID))
            );

        var courseResult = await _unitOfWork.Courses.GetAllWithCurrentVersionAsync(
            filter: filter,
            orderBy: q => q.OrderBy(c => c.CourseID),
            pageIndex: pageIndex,
            pageSize: pageSize);

        var courses = courseResult.Data.ToList();

        if (courses.Count == 0)
            return new PagedCourseBulkResponse
            {
                TotalItems = courseResult.TotalRecords,
                Items = []
            };

        var courseVersionIds = courses
            .Select(c => c.CurrentVersion!.CourseVersionID)
            .Distinct()
            .ToList();

        var participantCountByVersionId = await _unitOfWork.Enrollments
            .GetActiveOrCompletedParticipantCountsByCourseVersionIdsAsync(courseVersionIds);
        var ratingByVersionId = await _unitOfWork.Feedbacks
            .GetAverageRatingsByCourseVersionIdsAsync(courseVersionIds);

        var useInputOrder = searchRequest.CourseOwner == CourseOwnerFilter.Owned && ids.Count > 0;
        var idOrder = useInputOrder
            ? ids.Select((id, index) => new { id, index }).ToDictionary(x => x.id, x => x.index)
            : null;

        var data = courses
            .Select(c =>
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
                    Level = currentVersion.Level,
                    EstimatedDuration = currentVersion.EstimatedDuration,
                    Price = null,
                    ClubCourseOwned = new ClubCourseOwnedResponse(),
                    Rating = rating,
                    NumberOfParticipants = numberOfParticipants,
                    ImageUrl = currentVersion.ImageUrl
                };
            })
            .ToList();

        var orderedData = searchRequest.ParticipationSort switch
        {
            CourseParticipationSort.MostPopular when useInputOrder => data
                .OrderByDescending(x => x.NumberOfParticipants)
                .ThenBy(x => idOrder![x.CourseId]),

            CourseParticipationSort.MostPopular => data
                .OrderByDescending(x => x.NumberOfParticipants)
                .ThenBy(x => x.CourseId),

            CourseParticipationSort.LeastPopular when useInputOrder => data
                .OrderBy(x => x.NumberOfParticipants)
                .ThenBy(x => idOrder![x.CourseId]),

            CourseParticipationSort.LeastPopular => data
                .OrderBy(x => x.NumberOfParticipants)
                .ThenBy(x => x.CourseId),

            _ when useInputOrder => data.OrderBy(x => idOrder![x.CourseId]),

            _ => data.OrderBy(x => x.CourseId)
        };

        return new PagedCourseBulkResponse
        {
            TotalItems = courseResult.TotalRecords,
            Items = orderedData.ToList()
        };
    }

    public async Task<PagedCourseBulkResponse> GetHotCoursesByIdsAsync(
        HotCoursesSearchRequest searchRequest,
        IEnumerable<Guid> courseIds)
    {
        searchRequest ??= new HotCoursesSearchRequest();

        var pageIndex = searchRequest.CurrentPage < 1 ? 1 : searchRequest.CurrentPage;
        var pageSize = searchRequest.PageSize < 1 ? 5 : searchRequest.PageSize;

        var ids = courseIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

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
            level: null,
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

        var remainingCodeTask = _unitOfWork.Codes.GetAllAsync(
            filter: c =>
                itemCourseIds.Contains(c.CourseID) &&
                c.Status == CodeStatus.ACTIVE &&
                c.ExpireDate >= _clock.Now &&
                !c.CodeUsages.Any(),
            pageIndex: 1,
            pageSize: int.MaxValue);

        await Task.WhenAll(productTask, remainingCodeTask);

        var productByCourseId = productTask.Result
            .Where(p => p.ReferenceId != Guid.Empty)
            .ToDictionary(p => p.ReferenceId, p => p.Price);

        var remainingCodeByCourseId = remainingCodeTask.Result.Data
            .GroupBy(c => c.CourseID)
            .ToDictionary(g => g.Key, g => g.Count());

        var ownedCourseIdSet = ids.ToHashSet();

        foreach (var item in items)
        {
            item.Price = productByCourseId.TryGetValue(item.CourseId, out var price)
                ? price
                : 0m;

            if (ownedCourseIdSet.Contains(item.CourseId))
            {
                item.ClubCourseOwned = new ClubCourseOwnedResponse
                {
                    RemainingCode = remainingCodeByCourseId.TryGetValue(item.CourseId, out var remainingCode)
                        ? remainingCode
                        : 0,
                    ProfitType = item.Price > 0 ? ClubCourseProfit.PROFIT : ClubCourseProfit.NONPROFIT
                };
            }
            else
            {
                item.ClubCourseOwned = null;
            }
        }

        return new PagedCourseBulkResponse
        {
            TotalItems = courseResult.TotalRecords,
            Items = items
        };
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
