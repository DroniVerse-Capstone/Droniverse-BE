using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public class LessonService : ILessonService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LessonService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<LessonClientViewDTO> CreateLessonAsync(Guid moduleId, CreateLessonRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        await EnsureModuleExistsAsync(moduleId);
        await ValidateReferenceAsync(request.Type, request.ReferenceID);

        var lesson = _mapper.Map<Lesson>(request);
        lesson.LessonID = Guid.NewGuid();
        lesson.ModuleID = moduleId;

        await _unitOfWork.Lessons.AddAsync(lesson);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LessonClientViewDTO>(lesson);
    }

    public async Task<IEnumerable<LessonClientViewDTO>> GetLessonsByModuleAsync(Guid moduleId)
    {
        await EnsureModuleExistsAsync(moduleId);

        var lessons = await _unitOfWork.Lessons.GetAllAsync(
            filter: l => l.ModuleID == moduleId,
            orderBy: q => q.OrderBy(l => l.Type).ThenBy(l => l.LessonID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return _mapper.Map<IEnumerable<LessonClientViewDTO>>(lessons.Data);
    }

    public async Task<LessonClientViewDTO> GetLessonDetailAsync(Guid moduleId, Guid lessonId)
    {
        var lesson = await GetLessonAsync(moduleId, lessonId);
        return _mapper.Map<LessonClientViewDTO>(lesson);
    }

    public async Task<LessonClientViewDTO> UpdateLessonAsync(Guid moduleId, Guid lessonId, UpdateLessonRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var lesson = await GetLessonAsync(moduleId, lessonId);
        await ValidateReferenceAsync(request.Type, request.ReferenceID);

        _mapper.Map(request, lesson);

        await _unitOfWork.Lessons.UpdateAsync(lesson);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LessonClientViewDTO>(lesson);
    }

    public async Task DeleteLessonAsync(Guid moduleId, Guid lessonId)
    {
        var lesson = await GetLessonAsync(moduleId, lessonId);

        await _unitOfWork.Lessons.DeleteAsync(lesson);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task EnsureModuleExistsAsync(Guid moduleId)
    {
        var module = await _unitOfWork.Modules.GetByIdAsync(moduleId);
        if (module == null)
            throw new BaseException("Không tìm thấy mô-đun.", "NOT_FOUND");
    }

    private async Task<Lesson> GetLessonAsync(Guid moduleId, Guid lessonId)
    {
        await EnsureModuleExistsAsync(moduleId);

        var lesson = await _unitOfWork.Lessons.GetByConditionAsync(
            l => l.LessonID == lessonId && l.ModuleID == moduleId);

        if (lesson == null)
            throw new BaseException("Không tìm thấy bài học.", "NOT_FOUND");

        return lesson;
    }

    private async Task ValidateReferenceAsync(LessonType type, Guid referenceId)
    {
        if (referenceId == Guid.Empty)
            throw new ValidationException("ReferenceID là bắt buộc.");

        switch (type)
        {
            case LessonType.THEORY:
                if (await _unitOfWork.Theories.GetByIdAsync(referenceId) == null)
                    throw new ValidationException("Không tìm thấy tham chiếu bài lý thuyết.");
                break;
            case LessonType.QUIZ:
                if (await _unitOfWork.Quizs.GetByIdAsync(referenceId) == null)
                    throw new ValidationException("Không tìm thấy tham chiếu bài kiểm tra.");
                break;
            case LessonType.LAB:
                if (await _unitOfWork.Labs.GetByIdAsync(referenceId) == null)
                    throw new ValidationException("Không tìm thấy tham chiếu bài lab.");
                break;
            default:
                throw new ValidationException("Loại bài học không hợp lệ.");
        }
    }
}
