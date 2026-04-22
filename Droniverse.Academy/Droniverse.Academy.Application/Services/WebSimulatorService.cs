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

public class WebSimulatorService : IWebSimulatorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IUserDisplayNameService _userDisplayNameService;

    public WebSimulatorService(
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

    public async Task<WebSimulatorClientViewDTO> CreateWebSimulatorAsync(CreateWebSimulatorRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateData(
            request.TitleVN,
            request.TitleEN,
            request.Type,
            request.ObjectivesVN,
            request.ObjectivesEN,
            request.Code,
            request.EstimatedTime);

        await CourseVersionDraftGuard.EnsureDraftByModuleIdAsync(
            _unitOfWork,
            request.ModuleID,
            "Chỉ được chỉnh sửa lesson web simulator khi phiên bản khóa học ở trạng thái Draft.");

        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleID);
        if (module == null)
            throw new BaseException("Không tìm thấy mô-đun.", "NOT_FOUND");

        var orderIndex = request.OrderIndex ?? await GetNextOrderIndexAsync(request.ModuleID);
        await ValidateOrderIndexAsync(request.ModuleID, orderIndex);

        var webSimulator = new WebSimulator
        {
            WebSimulatorID = Guid.NewGuid(),
            TitleVN = request.TitleVN,
            TitleEN = request.TitleEN,
            Type = request.Type,
            ObjectivesVN = request.ObjectivesVN,
            ObjectivesEN = request.ObjectivesEN,
            Code = request.Code,
            EstimatedTime = request.EstimatedTime
        };
        webSimulator.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.WebSimulators.AddAsync(webSimulator);

        var lesson = new Lesson
        {
            LessonID = Guid.NewGuid(),
            ModuleID = request.ModuleID,
            OrderIndex = orderIndex,
            Type = LessonType.WEB,
            ReferenceID = webSimulator.WebSimulatorID
        };

        await _unitOfWork.Lessons.AddAsync(lesson);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToResponse(webSimulator);
        await PopulateUsersAsync(response, webSimulator.CreateBy, webSimulator.UpdateBy);

        return response;
    }

    public async Task<IEnumerable<WebSimulatorClientViewDTO>> GetWebSimulatorsAsync()
    {
        var webSimulators = await _unitOfWork.WebSimulators.GetAllAsync(
            orderBy: q => q.OrderByDescending(x => x.CreateAt),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var entities = webSimulators.Data.ToList();
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

    public async Task<WebSimulatorClientViewDTO> GetWebSimulatorByIdAsync(Guid webSimulatorId)
    {
        var webSimulator = await _unitOfWork.WebSimulators.GetByIdAsync(webSimulatorId);
        if (webSimulator == null)
            throw new BaseException("Không tìm thấy web simulator.", "NOT_FOUND");

        var response = MapToResponse(webSimulator);
        await PopulateUsersAsync(response, webSimulator.CreateBy, webSimulator.UpdateBy);

        return response;
    }

    public async Task<WebSimulatorClientViewDTO> UpdateWebSimulatorAsync(Guid webSimulatorId, UpdateWebSimulatorRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        await CourseVersionDraftGuard.EnsureDraftByReferenceAsync(
            _unitOfWork,
            webSimulatorId,
            LessonType.WEB,
            "web",
            "Chỉ được chỉnh sửa lesson web simulator khi phiên bản khóa học ở trạng thái Draft.");

        ValidateData(
            request.TitleVN,
            request.TitleEN,
            request.Type,
            request.ObjectivesVN,
            request.ObjectivesEN,
            request.Code,
            request.EstimatedTime);

        var webSimulator = await _unitOfWork.WebSimulators.GetByIdAsync(webSimulatorId);
        if (webSimulator == null)
            throw new BaseException("Không tìm thấy web simulator.", "NOT_FOUND");

        webSimulator.TitleVN = request.TitleVN;
        webSimulator.TitleEN = request.TitleEN;
        webSimulator.Type = request.Type;
        webSimulator.ObjectivesVN = request.ObjectivesVN;
        webSimulator.ObjectivesEN = request.ObjectivesEN;
        webSimulator.Code = request.Code;
        webSimulator.EstimatedTime = request.EstimatedTime;
        webSimulator.SetAuditOnUpdate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.WebSimulators.UpdateAsync(webSimulator);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToResponse(webSimulator);
        await PopulateUsersAsync(response, webSimulator.CreateBy, webSimulator.UpdateBy);

        return response;
    }

    public async Task DeleteWebSimulatorAsync(Guid webSimulatorId)
    {
        await CourseVersionDraftGuard.EnsureDraftByReferenceAsync(
            _unitOfWork,
            webSimulatorId,
            LessonType.WEB,
            "web",
            "Chỉ được chỉnh sửa lesson web simulator khi phiên bản khóa học ở trạng thái Draft.");

        var webSimulator = await _unitOfWork.WebSimulators.GetByIdAsync(webSimulatorId);
        if (webSimulator == null)
            throw new BaseException("Không tìm thấy web simulator.", "NOT_FOUND");

        var lesson = await _unitOfWork.Lessons.GetByConditionAsync(l => l.Type == LessonType.WEB && l.ReferenceID == webSimulator.WebSimulatorID);
        if (lesson != null)
        {
            lesson.ReferenceID = Guid.Empty;
            await _unitOfWork.Lessons.UpdateAsync(lesson);
        }

        await _unitOfWork.WebSimulators.DeleteAsync(webSimulator);
        await _unitOfWork.SaveChangesAsync();
    }

    private static void ValidateData(
        string titleVN,
        string titleEN,
        string type,
        string objectivesVN,
        string objectivesEN,
        string code,
        int estimatedTime)
    {
        if (string.IsNullOrWhiteSpace(titleVN))
            throw new ValidationException("Tiêu đề tiếng Việt là bắt buộc.");

        if (string.IsNullOrWhiteSpace(titleEN))
            throw new ValidationException("Tiêu đề tiếng Anh là bắt buộc.");

        if (string.IsNullOrWhiteSpace(type))
            throw new ValidationException("Loại web simulator là bắt buộc.");

        if (string.IsNullOrWhiteSpace(objectivesVN))
            throw new ValidationException("Mục tiêu tiếng Việt là bắt buộc.");

        if (objectivesVN.Length > 255)
            throw new ValidationException("Mục tiêu tiếng Việt không được vượt quá 255 ký tự.");

        if (string.IsNullOrWhiteSpace(objectivesEN))
            throw new ValidationException("Mục tiêu tiếng Anh là bắt buộc.");

        if (objectivesEN.Length > 255)
            throw new ValidationException("Mục tiêu tiếng Anh không được vượt quá 255 ký tự.");

        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Code web simulator là bắt buộc.");

        if (code.Length > 20)
            throw new ValidationException("Code web simulator không được vượt quá 20 ký tự.");

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

    private static WebSimulatorClientViewDTO MapToResponse(WebSimulator webSimulator)
    {
        return new WebSimulatorClientViewDTO
        {
            WebSimulatorID = webSimulator.WebSimulatorID,
            TitleVN = webSimulator.TitleVN,
            TitleEN = webSimulator.TitleEN,
            Type = webSimulator.Type,
            ObjectivesVN = webSimulator.ObjectivesVN,
            ObjectivesEN = webSimulator.ObjectivesEN,
            Code = webSimulator.Code,
            EstimatedTime = webSimulator.EstimatedTime,
            CreateAt = webSimulator.CreateAt,
            UpdateAt = webSimulator.UpdateAt
        };
    }

    private async Task PopulateUsersAsync(WebSimulatorClientViewDTO dto, Guid createBy, Guid updateBy)
    {
        var users = await _userDisplayNameService.GetListUserAsync(new[] { createBy, updateBy });
        var userLookup = users.ToDictionary(u => u.UserId, u => (SimpleUserReponse?)u);

        userLookup.TryGetValue(createBy, out var creator);
        userLookup.TryGetValue(updateBy, out var updater);

        dto.Creator = creator;
        dto.Updater = updater;
    }

    private async Task<Dictionary<Guid, SimpleUserReponse?>> BuildUserLookupAsync(IEnumerable<WebSimulator> entities)
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
