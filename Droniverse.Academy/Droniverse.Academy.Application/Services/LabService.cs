using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.IService.Mongo;
using Droniverse.Academy.Application.Validators;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;
using System.Linq.Expressions;

namespace Droniverse.Academy.Application.Services;

public class LabService : ILabService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILabContentService _labContentService;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IUserDisplayNameService _userDisplayNameService;

    public LabService(IUnitOfWork unitOfWork, ILabContentService labContentService, IMapper mapper, ICurrentUserService currentUser, IClock clock, IUserDisplayNameService userDisplayNameService)
    {
        _unitOfWork = unitOfWork;
        _labContentService = labContentService;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
        _userDisplayNameService = userDisplayNameService;
    }

    public async Task<LabDetailResponseDTO> CreateLabAsync(CreateLabRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        LabValidator.ValidateLabData(request.NameVN, request.NameEN, request.DescriptionVN, request.DescriptionEN);

        var lab = _mapper.Map<Lab>(request);
        lab.LabID = Guid.NewGuid();
        lab.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Labs.AddAsync(lab);

        await _unitOfWork.SaveChangesAsync();

        var labContent = await _labContentService.CreateEmptyAsync(lab.LabID);
        var mappedLab = _mapper.Map<LabClientViewDTO>(lab);
        await PopulateUsersAsync(mappedLab);

        return new LabDetailResponseDTO
        {
            Lab = mappedLab,
            LabContent = labContent
        };
    }

    public async Task<LessonClientViewDTO> CreateLessonFromLabAsync(Guid labId, CreateLabLessonRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

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

        return _mapper.Map<LessonClientViewDTO>(lesson);
    }

    public async Task<PaginationResult<IEnumerable<LabClientViewDTO>>> GetLabsAsync(GetLabsQueryDTO query)
    {
        if (query.PageIndex < 1) query.PageIndex = 1;
        if (query.PageSize < 1) query.PageSize = 10;

        Expression<Func<Lab, bool>>? filter = null;
        if (query.Status.HasValue)
        {
            var status = query.Status.Value;
            filter = x => x.Status == status;
        }

        var labs = await _unitOfWork.Labs.GetAllAsync(
            filter: filter,
            orderBy: q => q.OrderByDescending(l => l.UpdateAt),
            pageIndex: query.PageIndex,
            pageSize: query.PageSize);

        var mapped = _mapper.Map<List<LabClientViewDTO>>(labs.Data);
        await Task.WhenAll(mapped.Select(PopulateUsersAsync));

        return new PaginationResult<IEnumerable<LabClientViewDTO>>(mapped, labs.TotalRecords, labs.PageIndex, labs.PageSize);
    }

    public async Task<LabDetailResponseDTO> GetLabByIdAsync(Guid labId)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        var labContent = await _labContentService.GetByLabIdAsync(labId) ?? await _labContentService.CreateEmptyAsync(labId);
        var mappedLab = _mapper.Map<LabClientViewDTO>(lab);
        await PopulateUsersAsync(mappedLab);

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

        LabValidator.ValidateLabData(request.NameVN, request.NameEN, request.DescriptionVN, request.DescriptionEN);

        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        _mapper.Map(request, lab);
        lab.SetAuditOnUpdate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Labs.UpdateAsync(lab);
        await _unitOfWork.SaveChangesAsync();

        var labContent = await _labContentService.GetByLabIdAsync(labId) ?? await _labContentService.CreateEmptyAsync(labId);
        var mappedLab = _mapper.Map<LabClientViewDTO>(lab);
        await PopulateUsersAsync(mappedLab);

        return new LabDetailResponseDTO
        {
            Lab = mappedLab,
            LabContent = labContent
        };
    }

    public async Task<LabContentResponseDTO> UpdateLabContentAsync(Guid labId, UpdateLabContentRequestDTO request)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return await _labContentService.UpdateByLabIdAsync(labId, request);
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

    private async Task PopulateUsersAsync(LabClientViewDTO lab)
    {
        var (creator, updater) = await _userDisplayNameService.ResolveCreatorUpdaterAsync(lab.CreateBy, lab.UpdateBy);
        lab.Creator = creator;
        lab.Updater = updater;
    }
}
