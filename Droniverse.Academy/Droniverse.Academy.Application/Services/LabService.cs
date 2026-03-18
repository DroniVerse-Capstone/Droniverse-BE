using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.Validators;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;

namespace Droniverse.Academy.Application.Services;

public class LabService : ILabService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public LabService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<LabClientViewDTO> CreateLabAsync(CreateLabRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        LabValidator.ValidateLabData(request.NameVN, request.NameEN, request.DescriptionVN, request.DescriptionEN);

        var lesson = await _unitOfWork.Lessons.GetByIdAsync(request.LessonID);
        if (lesson == null)
            throw new BaseException("Không tìm thấy bài học.", "NOT_FOUND");

        if (lesson.Type != LessonType.LAB)
            throw new ValidationException("Loại bài học phải là LAB để gắn bài lab.");

        var existingLab = await _unitOfWork.Labs.GetByConditionAsync(l => l.LessonID == request.LessonID);
        if (existingLab != null)
            throw new ValidationException("Bài học này đã có bài lab.");

        var lab = _mapper.Map<Lab>(request);
        lab.LabID = Guid.NewGuid();
        lab.LessonID = request.LessonID;
        lab.CreateAt = _clock.Now;
        lab.UpdateAt = _clock.Now;
        lab.CreateBy = _currentUser.UserId;
        lab.UpdateBy = _currentUser.UserId;

        await _unitOfWork.Labs.AddAsync(lab);

        lesson.ReferenceID = lab.LabID;
        await _unitOfWork.Lessons.UpdateAsync(lesson);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LabClientViewDTO>(lab);
    }

    public async Task<IEnumerable<LabClientViewDTO>> GetLabsAsync()
    {
        var labs = await _unitOfWork.Labs.GetAllAsync(
            orderBy: q => q.OrderByDescending(l => l.UpdateAt),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return _mapper.Map<IEnumerable<LabClientViewDTO>>(labs.Data);
    }

    public async Task<LabClientViewDTO> GetLabByIdAsync(Guid labId)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        return _mapper.Map<LabClientViewDTO>(lab);
    }

    public async Task<LabClientViewDTO> UpdateLabAsync(Guid labId, UpdateLabRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        LabValidator.ValidateLabData(request.NameVN, request.NameEN, request.DescriptionVN, request.DescriptionEN);

        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        _mapper.Map(request, lab);
        lab.UpdateAt = _clock.Now;
        lab.UpdateBy = _currentUser.UserId;

        await _unitOfWork.Labs.UpdateAsync(lab);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LabClientViewDTO>(lab);
    }

    public async Task DeleteLabAsync(Guid labId)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(labId);
        if (lab == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        var lesson = await _unitOfWork.Lessons.GetByIdAsync(lab.LessonID);
        if (lesson == null)
            throw new BaseException("Không tìm thấy bài học của lab.", "NOT_FOUND");

        lesson.ReferenceID = Guid.Empty;
        await _unitOfWork.Lessons.UpdateAsync(lesson);

        await _unitOfWork.Labs.DeleteAsync(lab);
        await _unitOfWork.SaveChangesAsync();
    }
}
