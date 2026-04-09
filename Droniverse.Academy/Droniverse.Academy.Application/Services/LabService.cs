using AutoMapper;
using Droniverse.Academy.Application.Common.Extensions;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.IService.Mongo;
using Droniverse.Academy.Application.Validators;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Droniverse.Academy.Application.Services;

public class LabService : ILabService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILabContentService _labContentService;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IUserDisplayNameService _userDisplayNameService;
    private readonly ILogger<LabService> _logger;

    public LabService(
        IUnitOfWork unitOfWork,
        ILabContentService labContentService,
        IMapper mapper,
        ICurrentUserService currentUser,
        IClock clock,
        IUserDisplayNameService userDisplayNameService,
        ILogger<LabService> logger)
    {
        _unitOfWork = unitOfWork;
        _labContentService = labContentService;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
        _userDisplayNameService = userDisplayNameService;
        _logger = logger;
    }

    public async Task<LabDetailResponseDTO> CreateLabAsync(CreateLabRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        LabValidator.ValidateLabData(request.EstimatedTime, request.NameVN, request.NameEN, request.DescriptionVN, request.DescriptionEN);
        ValidateNameDifferent(request.NameVN, request.NameEN);
        await EnsureLabNamesUniqueAsync(request.NameVN, request.NameEN);

        var lab = _mapper.Map<Lab>(request);
        lab.LabID = Guid.NewGuid();
        lab.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Labs.AddAsync(lab);

        await _unitOfWork.SaveChangesAsync();

        var labContent = await _labContentService.CreateEmptyAsync(lab.LabID);
        var mappedLab = _mapper.Map<LabClientViewDTO>(lab);
        await PopulateUsersAsync(mappedLab, lab.CreateBy, lab.UpdateBy);

        return new LabDetailResponseDTO
        {
            Lab = mappedLab,
            LabContent = labContent
        };
    }

    public async Task<LabDetailResponseDTO> DuplicateLabAsync(Guid labId)
    {
        var sourceLab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (sourceLab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        var duplicatedLab = _mapper.Map<Lab>(sourceLab);
        duplicatedLab.LabID = Guid.NewGuid();
        duplicatedLab.Status = LabStatus.DRAFT;
        duplicatedLab.NameVN = $"{sourceLab.NameVN} (Copy)";
        duplicatedLab.NameEN = $"{sourceLab.NameEN} (Copy)";
        duplicatedLab.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Labs.AddAsync(duplicatedLab);
        await _unitOfWork.SaveChangesAsync();

        var sourceLabContent = await _labContentService.GetByLabIdAsync(sourceLab.LabID);
        var duplicatedLabContent = await _labContentService.CreateEmptyAsync(duplicatedLab.LabID);

        if (sourceLabContent != null)
        {
            var updateRequest = new UpdateLabContentRequestDTO
            {
                Environment = sourceLabContent.Environment.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
                    ? JsonDocument.Parse("{}").RootElement.Clone()
                    : sourceLabContent.Environment.Clone()
            };

            duplicatedLabContent = await _labContentService.UpdateByLabIdAsync(duplicatedLab.LabID, updateRequest);
        }

        var mappedLab = _mapper.Map<LabClientViewDTO>(duplicatedLab);
        await PopulateUsersAsync(mappedLab, duplicatedLab.CreateBy, duplicatedLab.UpdateBy);

        return new LabDetailResponseDTO
        {
            Lab = mappedLab,
            LabContent = duplicatedLabContent
        };
    }

    public async Task<LessonClientViewDTO> CreateLessonFromLabAsync(Guid labId, CreateLabLessonRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        if (lab.Status != LabStatus.ACTIVE)
            throw new ValidationException("Chỉ có thể thêm bài lab ở trạng thái Active vào lesson.");

        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleID);
        if (module == null)
            throw new BaseException("Không tìm thấy mô-đun.", "NOT_FOUND");

        var orderIndex = request.OrderIndex ?? await GetNextOrderIndexAsync(request.ModuleID);
        await ValidateOrderIndexAsync(request.ModuleID, orderIndex);

        var createLessonRequest = new CreateLessonRequestDTO
        {
            Type = LessonType.LAB,
            ReferenceID = labId
        };

        var lesson = _mapper.Map<Lesson>(createLessonRequest);
        lesson.LessonID = Guid.NewGuid();
        lesson.ModuleID = request.ModuleID;
        lesson.OrderIndex = orderIndex;

        await _unitOfWork.Lessons.AddAsync(lesson);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<LessonClientViewDTO>(lesson);
        response.TitleVN = lab.NameVN;
        response.TitleEN = lab.NameEN;
        response.EstimatedTime = lab.EstimatedTime;

        return response;
    }

    public async Task<PaginationResult<IEnumerable<LabClientViewDTO>>> GetLabsAsync(GetLabsQueryDTO query)
    {
        if (query.PageIndex < 1) query.PageIndex = 1;
        if (query.PageSize < 1) query.PageSize = 10;

        Expression<Func<Lab, bool>> filter = x => true;

        if (query.Type.HasValue)
        {
            var type = query.Type.Value;
            filter = filter.And(x => x.Type == type);
        }

        if (query.Status.HasValue)
        {
            var status = query.Status.Value;
            filter = filter.And(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var keyword = query.SearchTerm.Trim();
            filter = filter.And(x => x.NameVN.Contains(keyword) || x.NameEN.Contains(keyword));
        }

        var labs = await _unitOfWork.Labs.GetAllAsync(
            filter: filter,
            orderBy: q => q.OrderByDescending(l => l.UpdateAt),
            pageIndex: query.PageIndex,
            pageSize: query.PageSize);

        var entities = labs.Data.ToList();
        var mapped = _mapper.Map<List<LabClientViewDTO>>(entities);

        var userLookup = await BuildUserLookupAsync(entities);
        PopulateMappedLabsUsers(entities, mapped, userLookup);

        return new PaginationResult<IEnumerable<LabClientViewDTO>>(mapped, labs.TotalRecords, labs.PageIndex, labs.PageSize);
    }

    public async Task<LabDetailResponseDTO> GetLabByIdAsync(Guid labId)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        var labContent = await _labContentService.GetByLabIdAsync(labId) ?? await _labContentService.CreateEmptyAsync(labId);
        var mappedLab = _mapper.Map<LabClientViewDTO>(lab);
        await PopulateUsersAsync(mappedLab, lab.CreateBy, lab.UpdateBy);

        return new LabDetailResponseDTO
        {
            Lab = mappedLab,
            LabContent = labContent
        };
    }

    public async Task<LabDetailResponseDTO> UpdateLabAsync(Guid labId, UpdateLabRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        LabValidator.ValidateLabData(request.EstimatedTime, request.NameVN, request.NameEN, request.DescriptionVN, request.DescriptionEN);
        ValidateNameDifferent(request.NameVN, request.NameEN);

        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        if (lab.Status != LabStatus.DRAFT)
            throw new ValidationException("Chỉ có thể cập nhật lab ở trạng thái Draft.");

        await EnsureLabNamesUniqueAsync(request.NameVN, request.NameEN, labId);

        _mapper.Map(request, lab);
        lab.SetAuditOnUpdate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Labs.UpdateAsync(lab);
        await _unitOfWork.SaveChangesAsync();

        var labContent = await _labContentService.GetByLabIdAsync(labId) ?? await _labContentService.CreateEmptyAsync(labId);
        var mappedLab = _mapper.Map<LabClientViewDTO>(lab);
        await PopulateUsersAsync(mappedLab, lab.CreateBy, lab.UpdateBy);

        return new LabDetailResponseDTO
        {
            Lab = mappedLab,
            LabContent = labContent
        };
    }

    private static void ValidateNameDifferent(string nameVN, string nameEN)
    {
        if (string.Equals(nameVN?.Trim(), nameEN?.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("Tên tiếng Việt và tiếng Anh của lab phải khác nhau.");
    }

    private async Task EnsureLabNamesUniqueAsync(string nameVN, string nameEN, Guid? excludeLabId = null)
    {
        var normalizedVn = nameVN.Trim();
        var normalizedEn = nameEN.Trim();

        var duplicated = await _unitOfWork.Labs.GetByConditionAsync(l =>
            (!excludeLabId.HasValue || l.LabID != excludeLabId.Value) &&
            (l.NameVN == normalizedVn ||
             l.NameEN == normalizedVn ||
             l.NameVN == normalizedEn ||
             l.NameEN == normalizedEn));

        if (duplicated != null)
            throw new ValidationException("Tên lab bị trùng. NameVN/NameEN phải khác toàn bộ NameVN/NameEN của các lab khác.");
    }

    public async Task<LabContentResponseDTO> UpdateLabContentAsync(Guid labId, UpdateLabContentRequestDTO request)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        if (lab.Status != LabStatus.DRAFT)
            throw new ValidationException("Chỉ có thể cập nhật nội dung lab ở trạng thái Draft.");

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var updatedContent = await _labContentService.UpdateByLabIdAsync(labId, request);

        lab.SetAuditOnUpdate(_currentUser.UserId, _clock.Now);
        await _unitOfWork.Labs.UpdateAsync(lab);
        await _unitOfWork.SaveChangesAsync();

        return updatedContent;
    }

    public async Task DeleteLabAsync(Guid labId)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        var lessons = await _unitOfWork.Lessons.GetAllAsync(
            filter: l => l.Type == LessonType.LAB && l.ReferenceID == lab.LabID,
            pageIndex: 1,
            pageSize: int.MaxValue);

        foreach (var lesson in lessons.Data)
        {
            lesson.ReferenceID = Guid.Empty;
            await _unitOfWork.Lessons.UpdateAsync(lesson);
        }

        await _unitOfWork.Labs.DeleteAsync(lab);
        await _unitOfWork.SaveChangesAsync();

        await _labContentService.DeleteByLabIdAsync(labId);
    }

    public async Task<bool> IsLabExistAsync(Guid labId)
    {
        if (labId == Guid.Empty)
            return false;

        return await _unitOfWork.Labs.IsExistAsync(labId);
    }

    public async Task<IEnumerable<SimpleLabResponse>> GetLabsBulkAsync(IEnumerable<Guid> labIds)
    {
        return await _unitOfWork.Labs.GetSimpleLabsByIdsAsync(labIds);
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

    private async Task PopulateUsersAsync(LabClientViewDTO lab, Guid createBy, Guid updateBy)
    {
        var (creator, updater) = await _userDisplayNameService.ResolveCreatorUpdaterAsync(createBy, updateBy);
        lab.Creator = creator;
        lab.Updater = updater;
    }

    private async Task<Dictionary<Guid, SimpleUserReponse?>> BuildUserLookupAsync(IEnumerable<Lab> labs)
    {
        var userIds = labs
            .SelectMany(l => new[] { l.CreateBy, l.UpdateBy })
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        try
        {
            var lookup = await _userDisplayNameService.ResolveUsersDisplayNameAsync(userIds);
            return lookup.ToDictionary(x => x.Key, x => x.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không thể lấy thông tin người dùng từ Identity service. Trả về danh sách lab không kèm creator/updater.");
            return new Dictionary<Guid, SimpleUserReponse?>();
        }
    }

    private static void PopulateMappedLabsUsers(
        IEnumerable<Lab> entities,
        IEnumerable<LabClientViewDTO> dtos,
        IReadOnlyDictionary<Guid, SimpleUserReponse?> userLookup)
    {
        foreach (var (entity, dto) in entities.Zip(dtos))
        {
            if (userLookup.TryGetValue(entity.CreateBy, out var creator))
            {
                dto.Creator = creator;
            }

            if (userLookup.TryGetValue(entity.UpdateBy, out var updater))
            {
                dto.Updater = updater;
            }
        }
    }
}
