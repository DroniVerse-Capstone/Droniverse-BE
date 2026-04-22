using Droniverse.Academy.Application.Common.Extensions;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.Helpers;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class VRSimulatorService : IVRSimulatorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IUserDisplayNameService _userDisplayNameService;

    public VRSimulatorService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IClock clock,
        IUserDisplayNameService userDisplayNameService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
        _userDisplayNameService = userDisplayNameService;
    }

    public async Task<VRSimulatorClientViewDTO> CreateVRSimulatorAsync(CreateVRSimulatorRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateData(request.TitleVN, request.TitleEN, request.EstimatedTime);
        await EnsureTitlesUniqueAsync(request.TitleVN, request.TitleEN);

        var vrSimulator = new VRSimulator
        {
            VRSimulatorID = Guid.NewGuid(),
            TitleVN = request.TitleVN,
            TitleEN = request.TitleEN,
            EstimatedTime = request.EstimatedTime
        };
        vrSimulator.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.VRSimulators.AddAsync(vrSimulator);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToResponse(vrSimulator);
        await PopulateUsersAsync(response, vrSimulator.CreateBy, vrSimulator.UpdateBy);

        return response;
    }

    public async Task<LessonClientViewDTO> CreateLessonFromVRSimulatorAsync(Guid vrSimulatorId, CreateVRSimulatorLessonRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        await CourseVersionDraftGuard.EnsureDraftByModuleIdAsync(
            _unitOfWork,
            request.ModuleID,
            "Chỉ được chỉnh sửa lesson vr simulator khi phiên bản khóa học ở trạng thái Draft.");

        var vrSimulator = await _unitOfWork.VRSimulators.GetByIdAsync(vrSimulatorId);
        if (vrSimulator == null)
            throw new BaseException("Không tìm thấy vr simulator.", "NOT_FOUND");

        var orderIndex = request.OrderIndex ?? await GetNextOrderIndexAsync(request.ModuleID);
        await ValidateOrderIndexAsync(request.ModuleID, orderIndex);

        var lesson = new Lesson
        {
            LessonID = Guid.NewGuid(),
            ModuleID = request.ModuleID,
            OrderIndex = orderIndex,
            Type = LessonType.VR,
            ReferenceID = vrSimulator.VRSimulatorID
        };

        await _unitOfWork.Lessons.AddAsync(lesson);
        await _unitOfWork.SaveChangesAsync();

        return new LessonClientViewDTO
        {
            LessonID = lesson.LessonID,
            ModuleID = lesson.ModuleID,
            OrderIndex = lesson.OrderIndex,
            Type = lesson.Type,
            ReferenceID = lesson.ReferenceID,
            TitleVN = vrSimulator.TitleVN,
            TitleEN = vrSimulator.TitleEN,
            EstimatedTime = vrSimulator.EstimatedTime
        };
    }

    public async Task<IEnumerable<VRSimulatorClientViewDTO>> GetVRSimulatorsAsync()
    {
        var vrSimulators = await _unitOfWork.VRSimulators.GetAllAsync(
            orderBy: q => q.OrderByDescending(x => x.CreateAt),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var entities = vrSimulators.Data.ToList();
        var mapped = entities.Select(MapToResponse).ToList();

        var userLookup = await BuildUserLookupAsync(entities);
        foreach (var (entity, dto) in entities.Zip(mapped))
        {
            userLookup.TryGetValue(entity.CreateBy, out var creator);
            userLookup.TryGetValue(entity.UpdateBy, out var updater);
            dto.Creator = creator;
            dto.Updater = updater;
        }

        return mapped;
    }

    public async Task<VRSimulatorClientViewDTO> GetVRSimulatorByIdAsync(Guid vrSimulatorId)
    {
        var vrSimulator = await _unitOfWork.VRSimulators.GetByIdAsync(vrSimulatorId);
        if (vrSimulator == null)
            throw new BaseException("Không tìm thấy vr simulator.", "NOT_FOUND");

        var response = MapToResponse(vrSimulator);
        await PopulateUsersAsync(response, vrSimulator.CreateBy, vrSimulator.UpdateBy);

        return response;
    }

    public async Task<VRSimulatorClientViewDTO> UpdateVRSimulatorAsync(Guid vrSimulatorId, UpdateVRSimulatorRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateData(request.TitleVN, request.TitleEN, request.EstimatedTime);
        await EnsureTitlesUniqueAsync(request.TitleVN, request.TitleEN, vrSimulatorId);

        var vrSimulator = await _unitOfWork.VRSimulators.GetByIdAsync(vrSimulatorId);
        if (vrSimulator == null)
            throw new BaseException("Không tìm thấy vr simulator.", "NOT_FOUND");

        var mappedLessons = await GetMappedVRLessonsAsync(vrSimulatorId);
        foreach (var lesson in mappedLessons)
        {
            await CourseVersionDraftGuard.EnsureDraftByModuleIdAsync(
                _unitOfWork,
                lesson.ModuleID,
                "Chỉ được chỉnh sửa lesson vr simulator khi phiên bản khóa học ở trạng thái Draft.");
        }

        vrSimulator.TitleVN = request.TitleVN;
        vrSimulator.TitleEN = request.TitleEN;
        vrSimulator.EstimatedTime = request.EstimatedTime;
        vrSimulator.SetAuditOnUpdate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.VRSimulators.UpdateAsync(vrSimulator);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToResponse(vrSimulator);
        await PopulateUsersAsync(response, vrSimulator.CreateBy, vrSimulator.UpdateBy);

        return response;
    }

    public async Task DeleteVRSimulatorAsync(Guid vrSimulatorId)
    {
        var vrSimulator = await _unitOfWork.VRSimulators.GetByIdAsync(vrSimulatorId);
        if (vrSimulator == null)
            throw new BaseException("Không tìm thấy vr simulator.", "NOT_FOUND");

        var mappedLessons = await GetMappedVRLessonsAsync(vrSimulatorId);
        foreach (var lesson in mappedLessons)
        {
            await CourseVersionDraftGuard.EnsureDraftByModuleIdAsync(
                _unitOfWork,
                lesson.ModuleID,
                "Chỉ được chỉnh sửa lesson vr simulator khi phiên bản khóa học ở trạng thái Draft.");

            lesson.ReferenceID = Guid.Empty;
            await _unitOfWork.Lessons.UpdateAsync(lesson);
        }

        await _unitOfWork.VRSimulators.DeleteAsync(vrSimulator);
        await _unitOfWork.SaveChangesAsync();
    }

    private static void ValidateData(string titleVN, string titleEN, int estimatedTime)
    {
        if (string.IsNullOrWhiteSpace(titleVN))
            throw new ValidationException("Tiêu đề tiếng Việt là bắt buộc.");

        if (string.IsNullOrWhiteSpace(titleEN))
            throw new ValidationException("Tiêu đề tiếng Anh là bắt buộc.");

        if (estimatedTime <= 0)
            throw new ValidationException("EstimatedTime phải lớn hơn 0.");
    }

    private async Task<int> GetNextOrderIndexAsync(Guid moduleId)
    {
        var lessons = await _unitOfWork.Lessons.GetAllAsync(
            filter: l => l.ModuleID == moduleId,
            orderBy: q => q.OrderByDescending(l => l.OrderIndex),
            pageIndex: 1,
            pageSize: 1);

        var latest = lessons.Data.FirstOrDefault();
        return (latest?.OrderIndex ?? 0) + 1;
    }

    private async Task ValidateOrderIndexAsync(Guid moduleId, int orderIndex)
    {
        if (orderIndex <= 0)
            throw new ValidationException("OrderIndex phải lớn hơn 0.");

        var duplicated = await _unitOfWork.Lessons.GetByConditionAsync(
            l => l.ModuleID == moduleId && l.OrderIndex == orderIndex);

        if (duplicated != null)
            throw new ValidationException("OrderIndex phải là duy nhất trong mô-đun.");
    }

    private async Task<List<Lesson>> GetMappedVRLessonsAsync(Guid vrSimulatorId)
    {
        var lessons = await _unitOfWork.Lessons.GetAllAsync(
            filter: l => l.Type == LessonType.VR && l.ReferenceID == vrSimulatorId,
            pageIndex: 1,
            pageSize: int.MaxValue);

        return lessons.Data.ToList();
    }

    private async Task EnsureTitlesUniqueAsync(string titleVN, string titleEN, Guid? excludeVrSimulatorId = null)
    {
        var normalizedVn = titleVN.Trim();
        var normalizedEn = titleEN.Trim();

        var duplicated = await _unitOfWork.VRSimulators.GetByConditionAsync(vr =>
            (!excludeVrSimulatorId.HasValue || vr.VRSimulatorID != excludeVrSimulatorId.Value)
            && (vr.TitleVN == normalizedVn
                || vr.TitleEN == normalizedVn
                || vr.TitleVN == normalizedEn
                || vr.TitleEN == normalizedEn));

        if (duplicated != null)
            throw new ValidationException("Tiêu đề vr simulator bị trùng. TitleVN/TitleEN phải khác toàn bộ TitleVN/TitleEN của các vr simulator khác.");
    }

    private static VRSimulatorClientViewDTO MapToResponse(VRSimulator vrSimulator)
    {
        return new VRSimulatorClientViewDTO
        {
            VRSimulatorID = vrSimulator.VRSimulatorID,
            TitleVN = vrSimulator.TitleVN,
            TitleEN = vrSimulator.TitleEN,
            EstimatedTime = vrSimulator.EstimatedTime,
            CreateAt = vrSimulator.CreateAt,
            UpdateAt = vrSimulator.UpdateAt
        };
    }

    private async Task PopulateUsersAsync(VRSimulatorClientViewDTO dto, Guid createBy, Guid updateBy)
    {
        var users = await _userDisplayNameService.GetListUserAsync(new[] { createBy, updateBy });
        var userLookup = users.ToDictionary(u => u.UserId, u => (SimpleUserReponse?)u);

        userLookup.TryGetValue(createBy, out var creator);
        userLookup.TryGetValue(updateBy, out var updater);

        dto.Creator = creator;
        dto.Updater = updater;
    }

    private async Task<Dictionary<Guid, SimpleUserReponse?>> BuildUserLookupAsync(IEnumerable<VRSimulator> entities)
    {
        var userIds = entities
            .SelectMany(x => new[] { x.CreateBy, x.UpdateBy })
            .ToDistinctValidIds();

        var users = await _userDisplayNameService.GetListUserAsync(userIds);
        var lookup = users.ToDictionary(u => u.UserId, u => (SimpleUserReponse?)u);

        foreach (var userId in userIds)
        {
            lookup.TryAdd(userId, null);
        }

        return lookup;
    }
}
