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
        lesson.OrderIndex = request.OrderIndex ?? await GetNextOrderIndexAsync(moduleId);

        await ValidateOrderIndexAsync(moduleId, lesson.OrderIndex);

        await _unitOfWork.Lessons.AddAsync(lesson);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LessonClientViewDTO>(lesson);
    }

    public async Task<IEnumerable<LessonClientViewDTO>> GetLessonsByModuleAsync(Guid moduleId)
    {
        await EnsureModuleExistsAsync(moduleId);

        var lessons = await _unitOfWork.Lessons.GetAllAsync(
            filter: l => l.ModuleID == moduleId,
            orderBy: q => q.OrderBy(l => l.OrderIndex).ThenBy(l => l.LessonID),
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

        if (request.OrderIndex.HasValue && request.OrderIndex.Value != lesson.OrderIndex)
        {
            await ValidateOrderIndexAsync(moduleId, request.OrderIndex.Value, lessonId);
            lesson.OrderIndex = request.OrderIndex.Value;
        }

        _mapper.Map(request, lesson);

        await _unitOfWork.Lessons.UpdateAsync(lesson);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<LessonClientViewDTO>(lesson);
    }

    public async Task<IEnumerable<LessonClientViewDTO>> ReorderLessonsAsync(Guid moduleId, ReorderLessonsRequestDTO request)
    {
        await EnsureModuleExistsAsync(moduleId);

        if (request.Lessons.Count == 0)
            throw new ValidationException("Dữ liệu sắp xếp lại bài học là bắt buộc.");

        if (request.Lessons.Select(x => x.LessonID).Distinct().Count() != request.Lessons.Count)
            throw new ValidationException("Dữ liệu sắp xếp lại chứa lessonId bị trùng.");

        if (request.Lessons.Select(x => x.OrderIndex).Distinct().Count() != request.Lessons.Count)
            throw new ValidationException("OrderIndex phải là duy nhất trong dữ liệu sắp xếp lại.");

        if (request.Lessons.Any(x => x.OrderIndex <= 0))
            throw new ValidationException("OrderIndex phải lớn hơn 0.");

        var lessonsResult = await _unitOfWork.Lessons.GetAllAsync(
            filter: l => l.ModuleID == moduleId,
            orderBy: q => q.OrderBy(l => l.OrderIndex),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var lessons = lessonsResult.Data.ToList();
        if (lessons.Count != request.Lessons.Count)
            throw new ValidationException("Dữ liệu sắp xếp lại phải chứa đầy đủ tất cả bài học của mô-đun.");

        var lessonIds = lessons.Select(l => l.LessonID).OrderBy(x => x).ToList();
        var requestIds = request.Lessons.Select(l => l.LessonID).OrderBy(x => x).ToList();
        if (!lessonIds.SequenceEqual(requestIds))
            throw new ValidationException("Dữ liệu sắp xếp lại chứa lessonId không hợp lệ.");

        var reorderMap = request.Lessons.ToDictionary(x => x.LessonID, x => x.OrderIndex);

        foreach (var lesson in lessons)
        {
            lesson.OrderIndex = reorderMap[lesson.LessonID];
            await _unitOfWork.Lessons.UpdateAsync(lesson);
        }

        await _unitOfWork.SaveChangesAsync();

        var ordered = lessons.OrderBy(l => l.OrderIndex).ToList();
        return _mapper.Map<IEnumerable<LessonClientViewDTO>>(ordered);
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
            return;

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

    private async Task ValidateOrderIndexAsync(Guid moduleId, int orderIndex, Guid? excludeLessonId = null)
    {
        if (orderIndex <= 0)
            throw new ValidationException("OrderIndex phải lớn hơn 0.");

        var duplicated = await _unitOfWork.Lessons.GetByConditionAsync(
            l => l.ModuleID == moduleId
                 && l.OrderIndex == orderIndex
                 && (!excludeLessonId.HasValue || l.LessonID != excludeLessonId.Value));

        if (duplicated != null)
            throw new ValidationException("OrderIndex phải là duy nhất trong mô-đun.");
    }
}
