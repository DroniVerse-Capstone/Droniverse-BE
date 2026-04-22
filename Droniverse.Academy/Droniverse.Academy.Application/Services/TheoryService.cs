using AutoMapper;
using Droniverse.Academy.Application.Common.Extensions;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.Helpers;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.Validators;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class TheoryService : ITheoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IUserLookupService _userLookupService;

    public TheoryService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IClock clock, IUserLookupService userLookupService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
        _userLookupService = userLookupService;
    }

    public async Task<TheoryClientViewDTO> CreateTheoryAsync(CreateTheoryRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        TheoryValidator.ValidateTheoryData(request.EstimatedTime, request.TitleVN, request.TitleEN, request.ContentVN, request.ContentEN);

        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleID);
        if (module == null)
            throw new BaseException("Không tìm thấy mô-đun.", "NOT_FOUND");

        await CourseVersionDraftGuard.EnsureDraftByModuleIdAsync(
            _unitOfWork,
            request.ModuleID,
            "Chỉ được chỉnh sửa lesson lý thuyết khi phiên bản khóa học ở trạng thái Draft.");

        var orderIndex = request.OrderIndex ?? await GetNextOrderIndexAsync(request.ModuleID);
        await ValidateOrderIndexAsync(request.ModuleID, orderIndex);

        var theory = _mapper.Map<Theory>(request);
        theory.TheoryID = Guid.NewGuid();
        theory.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Theories.AddAsync(theory);

        var lesson = new Lesson
        {
            LessonID = Guid.NewGuid(),
            ModuleID = request.ModuleID,
            OrderIndex = orderIndex,
            Type = LessonType.THEORY,
            ReferenceID = theory.TheoryID
        };

        await _unitOfWork.Lessons.AddAsync(lesson);

        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<TheoryClientViewDTO>(theory);
        await PopulateUsersAsync(response, theory.CreateBy, theory.UpdateBy);

        return response;
    }

    public async Task<IEnumerable<TheoryClientViewDTO>> GetTheoriesAsync()
    {
        var theories = await _unitOfWork.Theories.GetAllAsync(
            orderBy: q => q.OrderByDescending(x => x.CreateAt),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var entities = theories.Data.ToList();
        var mapped = _mapper.Map<List<TheoryClientViewDTO>>(entities);

        var userLookup = await BuildUserLookupAsync(entities);
        PopulateMappedTheoriesUsers(entities, mapped, userLookup);

        return mapped;
    }

    public async Task<TheoryClientViewDTO> GetTheoryByIdAsync(Guid theoryId)
    {
        var theory = await _unitOfWork.Theories.GetByIdAsync(theoryId);
        if (theory == null)
            throw new BaseException("Không tìm thấy bài lý thuyết.", "NOT_FOUND");

        var response = _mapper.Map<TheoryClientViewDTO>(theory);
        await PopulateUsersAsync(response, theory.CreateBy, theory.UpdateBy);

        return response;
    }

    public async Task<TheoryClientViewDTO> UpdateTheoryAsync(Guid theoryId, UpdateTheoryRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        await CourseVersionDraftGuard.EnsureDraftByReferenceAsync(
            _unitOfWork,
            theoryId,
            LessonType.THEORY,
            "lý thuyết",
            "Chỉ được chỉnh sửa lesson lý thuyết khi phiên bản khóa học ở trạng thái Draft.");

        TheoryValidator.ValidateTheoryData(request.EstimatedTime, request.TitleVN, request.TitleEN, request.ContentVN, request.ContentEN);

        var theory = await _unitOfWork.Theories.GetByIdAsync(theoryId);
        if (theory == null)
            throw new BaseException("Không tìm thấy bài lý thuyết.", "NOT_FOUND");

        _mapper.Map(request, theory);
        theory.SetAuditOnUpdate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Theories.UpdateAsync(theory);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<TheoryClientViewDTO>(theory);
        await PopulateUsersAsync(response, theory.CreateBy, theory.UpdateBy);

        return response;
    }

    public async Task DeleteTheoryAsync(Guid theoryId)
    {
        await CourseVersionDraftGuard.EnsureDraftByReferenceAsync(
            _unitOfWork,
            theoryId,
            LessonType.THEORY,
            "lý thuyết",
            "Chỉ được chỉnh sửa lesson lý thuyết khi phiên bản khóa học ở trạng thái Draft.");

        var theory = await _unitOfWork.Theories.GetByIdAsync(theoryId);
        if (theory == null)
            throw new BaseException("Không tìm thấy bài lý thuyết.", "NOT_FOUND");

        var lesson = await _unitOfWork.Lessons.GetByConditionAsync(l => l.Type == LessonType.THEORY && l.ReferenceID == theory.TheoryID);
        if (lesson != null && lesson.ReferenceID == theory.TheoryID)
        {
            lesson.ReferenceID = Guid.Empty;
            await _unitOfWork.Lessons.UpdateAsync(lesson);
        }

        await _unitOfWork.Theories.DeleteAsync(theory);
        await _unitOfWork.SaveChangesAsync();
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

    private async Task PopulateUsersAsync(TheoryClientViewDTO theory, Guid createBy, Guid updateBy)
    {
        var userLookup = await _userLookupService.BuildUserLookupAsync(new[] { createBy, updateBy });

        userLookup.TryGetValue(createBy, out var creator);
        userLookup.TryGetValue(updateBy, out var updater);

        theory.Creator = creator;
        theory.Updater = updater;
    }

    private async Task<Dictionary<Guid, SimpleUserReponse?>> BuildUserLookupAsync(IEnumerable<Theory> theories)
    {
        var userIds = theories
            .SelectMany(t => new[] { t.CreateBy, t.UpdateBy })
            .ToDistinctValidIds();

        return await _userLookupService.BuildUserLookupAsync(userIds);
    }

    private static void PopulateMappedTheoriesUsers(
        IEnumerable<Theory> entities,
        IEnumerable<TheoryClientViewDTO> dtos,
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
