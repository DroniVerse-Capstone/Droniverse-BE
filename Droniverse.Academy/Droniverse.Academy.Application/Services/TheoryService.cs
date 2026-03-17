using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;

namespace Droniverse.Academy.Application.Services;

public class TheoryService : ITheoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public TheoryService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<TheoryClientViewDTO> CreateTheoryAsync(CreateTheoryRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateTheoryData(request.EstimatedTime, request.ContentVN, request.ContentEN);

        var lesson = await _unitOfWork.Lessons.GetByIdAsync(request.LessonID);
        if (lesson == null)
            throw new BaseException("Lesson not found.", "NOT_FOUND");

        if (lesson.Type != LessonType.THEORY)
            throw new ValidationException("Lesson type must be THEORY to attach theory.");

        var existingTheory = await _unitOfWork.Theories.GetByConditionAsync(t => t.LessonID == request.LessonID);
        if (existingTheory != null)
            throw new ValidationException("This lesson already has a theory.");

        var theory = _mapper.Map<Theory>(request);
        theory.TheoryID = Guid.NewGuid();
        theory.CreateAt = _clock.Now;
        theory.UpdateAt = _clock.Now;
        theory.CreateBy = _currentUser.UserId;
        theory.UpdateBy = _currentUser.UserId;

        await _unitOfWork.Theories.AddAsync(theory);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TheoryClientViewDTO>(theory);
    }

    public async Task<IEnumerable<TheoryClientViewDTO>> GetTheoriesAsync()
    {
        var theories = await _unitOfWork.Theories.GetAllAsync(
            orderBy: q => q.OrderByDescending(x => x.CreateAt),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return _mapper.Map<IEnumerable<TheoryClientViewDTO>>(theories.Data);
    }

    public async Task<TheoryClientViewDTO> GetTheoryByIdAsync(Guid theoryId)
    {
        var theory = await _unitOfWork.Theories.GetByIdAsync(theoryId);
        if (theory == null)
            throw new BaseException("Theory not found.", "NOT_FOUND");

        return _mapper.Map<TheoryClientViewDTO>(theory);
    }

    public async Task<TheoryClientViewDTO> UpdateTheoryAsync(Guid theoryId, UpdateTheoryRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateTheoryData(request.EstimatedTime, request.ContentVN, request.ContentEN);

        var theory = await _unitOfWork.Theories.GetByIdAsync(theoryId);
        if (theory == null)
            throw new BaseException("Theory not found.", "NOT_FOUND");

        _mapper.Map(request, theory);
        theory.UpdateAt = _clock.Now;
        theory.UpdateBy = _currentUser.UserId;

        await _unitOfWork.Theories.UpdateAsync(theory);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<TheoryClientViewDTO>(theory);
    }

    public async Task DeleteTheoryAsync(Guid theoryId)
    {
        var theory = await _unitOfWork.Theories.GetByIdAsync(theoryId);
        if (theory == null)
            throw new BaseException("Theory not found.", "NOT_FOUND");

        await _unitOfWork.Theories.DeleteAsync(theory);
        await _unitOfWork.SaveChangesAsync();
    }

    private static void ValidateTheoryData(int estimatedTime, string contentVN, string contentEN)
    {
        if (string.IsNullOrWhiteSpace(contentVN))
            throw new ValidationException("ContentVN is required.");

        if (string.IsNullOrWhiteSpace(contentEN))
            throw new ValidationException("ContentEN is required.");

        if (estimatedTime <= 0)
            throw new ValidationException("EstimatedTime must be greater than 0.");
    }
}
