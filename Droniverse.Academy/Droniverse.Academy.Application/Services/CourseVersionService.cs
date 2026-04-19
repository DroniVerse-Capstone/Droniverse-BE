using AutoMapper;
using Droniverse.Academy.Application.Common.Extensions;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.HttpClients;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using System.Linq.Expressions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class CourseVersionService : ICourseVersionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IMapper _mapper;
    private readonly IUserDisplayNameService _userDisplayNameService;
    private readonly CommunityMicroserviceClient _communityMicroserviceClient;
    private readonly ICourseVersionDuplicator _courseVersionDuplicator;
    private readonly ILabContentSyncService _labContentSyncService;

    public CourseVersionService(
        IUnitOfWork unitOfWork,
        ICurrentUserService current,
        IClock clock,
        IMapper mapper,
        IUserDisplayNameService userDisplayNameService,
        CommunityMicroserviceClient communityMicroserviceClient,
        ICourseVersionDuplicator courseVersionDuplicator,
        ILabContentSyncService labContentSyncService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = current;
        _clock = clock;
        _mapper = mapper;
        _userDisplayNameService = userDisplayNameService;
        _communityMicroserviceClient = communityMicroserviceClient;
        _courseVersionDuplicator = courseVersionDuplicator;
        _labContentSyncService = labContentSyncService;
    }

    public async Task<CourseVersionResponseDTO> CreateCourseVersionAsync(Guid courseId, CreateCourseVersionRequestDTO request)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        if (course.Status == CourseStatus.ARCHIVED)
            throw new ValidationException("Khóa học đã lưu trữ chỉ được xem, không thể thao tác.");

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
        cv.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        if (nextVersion == 1)
        {
            course.CurrentVersion = cv;
            course.CurrentVersionID = cv.CourseVersionID;
        }

        await _unitOfWork.CourseVersions.AddAsync(cv);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<CourseVersionResponseDTO>(cv);
        response.Certificate = _mapper.Map<CertificateVersionResponseDTO?>(cv.Certificate);
        response.Updater = await ResolveUpdaterAsync(cv.UpdateBy);

        return response;
    }

    public async Task DeleteCourseVersionAsync(Guid courseId, Guid versionId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        if (course.Status == CourseStatus.ARCHIVED)
            throw new ValidationException("Khóa học đã lưu trữ chỉ được xem, không thể thao tác.");

        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(v => v.CourseVersionID == versionId && v.CourseID == courseId);
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (course.CurrentVersionID == versionId)
            throw new ValidationException("Không thể xóa phiên bản hiện tại.");

        if (cv.Status == CourseVersionStatus.INACTIVE)
            throw new ValidationException("Phiên bản đã ở trạng thái đã xóa mềm, chỉ được xem.");

        if (cv.Status != CourseVersionStatus.DRAFT && cv.Status != CourseVersionStatus.DEPRECATED)
            throw new ValidationException("Chỉ phiên bản ở trạng thái Draft hoặc Deprecated mới có thể xóa mềm.");

        cv.Inactivate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.CourseVersions.UpdateAsync(cv);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CourseVersionResponseDTO> GetCourseVersionByIdAsync(Guid courseId, Guid versionId)
    {
        var cv = await _unitOfWork.CourseVersions.GetByConditionAsync(v => v.CourseVersionID == versionId && v.CourseID == courseId, includeProperties: "Certificate");
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        var response = _mapper.Map<CourseVersionResponseDTO>(cv);
        response.Certificate = _mapper.Map<CertificateVersionResponseDTO?>(cv.Certificate);
        response.Updater = await ResolveUpdaterAsync(cv.UpdateBy);
        response.Categories = [];
        response.RequiredDrones = [];

        return response;
    }

    public async Task<CourseVersionResponseDTO> DuplicateCourseVersionAsync(Guid courseId, Guid versionId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        if (course.Status == CourseStatus.ARCHIVED)
            throw new ValidationException("Khóa học đã lưu trữ chỉ được xem, không thể thao tác.");

        var sourceVersion = course.CourseVersions.FirstOrDefault(v => v.CourseVersionID == versionId);
        if (sourceVersion == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (sourceVersion.Status == CourseVersionStatus.INACTIVE)
            throw new ValidationException("Không thể nhân bản phiên bản đã xóa mềm.");

        var nextVersion = course.CourseVersions.Any()
            ? course.CourseVersions.Max(v => v.Version) + 1
            : 1;

        // Sao chép Version của khóa học, đồng thời sao chép các nội dung liên quan như Module, Lesson, Theory, Quiz, Lab và các bảng liên quan như Category, RequiredDrone. Kết quả trả về bao gồm phiên bản đã sao chép và danh sách các Lab cần đồng bộ nội dung.
        var now = _clock.Now;
        var duplicationResult = await _courseVersionDuplicator.DuplicateAsync(
            course,
            sourceVersion,
            nextVersion,
            _currentUser.UserId,
            now);

        // Đồng bộ nội dung Lab. Nếu có lỗi xảy ra trong quá trình đồng bộ, sẽ thực hiện dọn dẹp các Lab đã được tạo mới để tránh dữ liệu không nhất quán.
        try
        {
            
            await _labContentSyncService.SyncAsync(duplicationResult.LabContentSyncQueue);
            await _unitOfWork.SaveChangesAsync();
        }
        catch
        {
            await _labContentSyncService.CleanupAsync(duplicationResult.LabContentSyncQueue.Select(x => x.NewLabId));
            throw;
        }

        var response = _mapper.Map<CourseVersionResponseDTO>(duplicationResult.DuplicatedVersion);
        response.Certificate = _mapper.Map<CertificateVersionResponseDTO?>(duplicationResult.DuplicatedVersion.Certificate);
        response.Updater = await ResolveUpdaterAsync(duplicationResult.DuplicatedVersion.UpdateBy);
        return response;
    }

    public async Task<PaginationResult<IEnumerable<CourseVersionResponseDTO>>> GetCourseVersionsAsync(Guid courseId, int pageIndex, int pageSize, CourseVersionStatus? status = null)
    {
        Expression<Func<CourseVersion, bool>>? filter = v => v.CourseID == courseId;
        if (status.HasValue)
        {
            var s = status.Value;
            filter = v => v.CourseID == courseId && v.Status == s;
        }

        var result = await _unitOfWork.CourseVersions.GetAllAsync(filter, null, pageIndex, pageSize, includeProperties: "Certificate");
        var entities = result.Data.ToList();
        var mapped = entities.Select(v => _mapper.Map<CourseVersionResponseDTO>(v)).ToList();

        var userLookup = await BuildUpdaterLookupAsync(entities);
        mapped = MapVersionsUpdater(entities, mapped, userLookup);
        mapped = MapVersionsCertificates(entities, mapped);

        return new PaginationResult<IEnumerable<CourseVersionResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task ActivateCourseVersionAsync(Guid courseId, Guid versionId)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        if (course.Status == CourseStatus.ARCHIVED)
            throw new ValidationException("Khóa học đã lưu trữ chỉ được xem, không thể thao tác.");

        var cv = course.CourseVersions.FirstOrDefault(v => v.CourseVersionID == versionId);
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (cv.Status == CourseVersionStatus.INACTIVE)
            throw new ValidationException("Phiên bản đã xóa mềm chỉ được xem, không thể thao tác.");

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
        throw new ValidationException("Không hỗ trợ vô hiệu hóa trực tiếp phiên bản. Hãy kích hoạt phiên bản khác để phiên bản đang Active chuyển sang Deprecated.");
    }

    public async Task<CourseVersionResponseDTO> UpdateCourseVersionAsync(Guid courseId, Guid versionId, UpdateCourseVersionRequestDTO request)
    {
        var course = await _unitOfWork.Courses.GetByIdWithAllVersionsAsync(courseId);
        if (course == null)
            throw new BaseException("Không tìm thấy khóa học.", "NOT_FOUND");

        if (course.Status == CourseStatus.ARCHIVED)
            throw new ValidationException("Khóa học đã lưu trữ chỉ được xem, không thể thao tác.");

        var cv = course.CourseVersions.FirstOrDefault(v => v.CourseVersionID == versionId);
        if (cv == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (cv.Status == CourseVersionStatus.INACTIVE)
            throw new ValidationException("Phiên bản đã xóa mềm chỉ được xem, không thể thao tác.");

        cv.UpdateContent(request.TitleVN, request.TitleEN, request.DescriptionVN, request.DescriptionEN, request.ContextVN, request.ContextEN, request.ImageUrl, request.EstimatedDuration, request.ChangeLog, _currentUser.UserId, _clock.Now);

        await _unitOfWork.CourseVersions.UpdateAsync(cv);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<CourseVersionResponseDTO>(cv);
        response.Certificate = _mapper.Map<CertificateVersionResponseDTO?>(cv.Certificate);
        response.Updater = await ResolveUpdaterAsync(cv.UpdateBy);

        return response;
    }

    private async Task<SimpleUserReponse?> ResolveUpdaterAsync(Guid? updateBy)
    {
        if (!updateBy.HasValue || updateBy.Value == Guid.Empty)
            return null;

        var users = await _userDisplayNameService.GetListUserAsync(new[] { updateBy.Value });
        return users.FirstOrDefault();
    }

    private async Task<Dictionary<Guid, SimpleUserReponse?>> BuildUpdaterLookupAsync(IEnumerable<CourseVersion> versions)
    {
        var userIds = versions
            .Select(v => v.UpdateBy)
            .ToDistinctValidIds();

        var users = await _userDisplayNameService.GetListUserAsync(userIds);
        var lookup = users.ToDictionary(u => u.UserId, u => (SimpleUserReponse?)u);

        foreach (var userId in userIds)
        {
            lookup.TryAdd(userId, null);
        }

        return lookup;
    }

    private static List<CourseVersionResponseDTO> MapVersionsUpdater(
        IEnumerable<CourseVersion> entities,
        List<CourseVersionResponseDTO> dtos,
        IReadOnlyDictionary<Guid, SimpleUserReponse?> userLookup)
    {
        foreach (var (entity, dto) in entities.Zip(dtos))
        {
            var updaterId = entity.UpdateBy;
            if (updaterId.HasValue && updaterId.Value != Guid.Empty
                && userLookup.TryGetValue(updaterId.Value, out var updater))
            {
                dto.Updater = updater;
            }
        }

        return dtos;
    }

    private List<CourseVersionResponseDTO> MapVersionsCertificates(
        IEnumerable<CourseVersion> entities,
        List<CourseVersionResponseDTO> dtos)
    {
        foreach (var (entity, dto) in entities.Zip(dtos))
        {
            dto.Certificate = _mapper.Map<CertificateVersionResponseDTO?>(entity.Certificate);
        }

        return dtos;
    }

}
