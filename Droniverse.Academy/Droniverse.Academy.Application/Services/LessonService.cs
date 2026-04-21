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

        return await MapLessonWithReferenceAsync(lesson);
    }

    public async Task<IEnumerable<LessonClientViewDTO>> GetLessonsByModuleAsync(Guid moduleId)
    {
        await EnsureModuleExistsAsync(moduleId);

        var lessons = await _unitOfWork.Lessons.GetAllAsync(
            filter: l => l.ModuleID == moduleId,
            orderBy: q => q.OrderBy(l => l.OrderIndex).ThenBy(l => l.LessonID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return await MapLessonsWithReferenceAsync(lessons.Data);
    }

    public async Task<LessonClientViewDTO> GetLessonDetailAsync(Guid moduleId, Guid lessonId)
    {
        var lesson = await GetLessonAsync(moduleId, lessonId);
        return await MapLessonWithReferenceAsync(lesson);
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

        return await MapLessonWithReferenceAsync(lesson);
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
        return await MapLessonsWithReferenceAsync(ordered);
    }

    private async Task<LessonClientViewDTO> MapLessonWithReferenceAsync(Lesson lesson)
    {
        var mapped = _mapper.Map<LessonClientViewDTO>(lesson);
        await PopulateReferenceInfoAsync(lesson, mapped);
        return mapped;
    }

    private async Task<List<LessonClientViewDTO>> MapLessonsWithReferenceAsync(IEnumerable<Lesson> lessons)
    {
        var lessonList = lessons.ToList();
        var mapped = _mapper.Map<List<LessonClientViewDTO>>(lessonList);

        if (lessonList.Count == 0)
            return mapped;

        var lookups = await BuildReferenceLookupsAsync(lessonList);

        for (var index = 0; index < lessonList.Count; index++)
        {
            PopulateReferenceInfo(lessonList[index], mapped[index], lookups);
        }

        return mapped;
    }

    private async Task PopulateReferenceInfoAsync(Lesson lesson, LessonClientViewDTO dto)
    {
        if (lesson.ReferenceID == Guid.Empty)
            return;

        switch (lesson.Type)
        {
            case LessonType.THEORY:
                var theory = await _unitOfWork.Theories.GetByIdAsync(lesson.ReferenceID);
                if (theory != null)
                {
                    MapTheoryToDto(theory, dto);
                }
                break;
            case LessonType.QUIZ:
                var quiz = await _unitOfWork.Quizs.GetByIdAsync(lesson.ReferenceID);
                if (quiz != null)
                {
                    MapQuizToDto(quiz, dto);
                }
                break;
            case LessonType.LAB:
                var lab = await _unitOfWork.Labs.GetByIdAsync(lesson.ReferenceID);
                if (lab != null)
                {
                    MapLabToDto(lab, dto);
                }
                break;
            case LessonType.WEB:
                var webSimulator = await _unitOfWork.WebSimulators.GetByIdAsync(lesson.ReferenceID);
                if (webSimulator != null)
                {
                    MapWebSimulatorToDto(webSimulator, dto);
                }
                break;
            case LessonType.VR:
                var vrSimulator = await _unitOfWork.VRSimulators.GetByIdAsync(lesson.ReferenceID);
                if (vrSimulator != null)
                {
                    MapVRSimulatorToDto(vrSimulator, dto);
                }
                break;
        }
    }

    private async Task<ReferenceLookups> BuildReferenceLookupsAsync(List<Lesson> lessons)
    {
        var theoryIds = GetReferenceIdsByType(lessons, LessonType.THEORY);
        var quizIds = GetReferenceIdsByType(lessons, LessonType.QUIZ);
        var labIds = GetReferenceIdsByType(lessons, LessonType.LAB);
        var webSimulatorIds = GetReferenceIdsByType(lessons, LessonType.WEB);
        var vrSimulatorIds = GetReferenceIdsByType(lessons, LessonType.VR);

        var theoryLookup = await GetTheoryLookupAsync(theoryIds);
        var quizLookup = await GetQuizLookupAsync(quizIds);
        var labLookup = await GetLabLookupAsync(labIds);
        var webSimulatorLookup = await GetWebSimulatorLookupAsync(webSimulatorIds);
        var vrSimulatorLookup = await GetVRSimulatorLookupAsync(vrSimulatorIds);

        return new ReferenceLookups(theoryLookup, quizLookup, labLookup, webSimulatorLookup, vrSimulatorLookup);
    }

    private static HashSet<Guid> GetReferenceIdsByType(IEnumerable<Lesson> lessons, LessonType type)
    {
        return lessons
            .Where(l => l.Type == type && l.ReferenceID != Guid.Empty)
            .Select(l => l.ReferenceID)
            .ToHashSet();
    }

    private async Task<Dictionary<Guid, Theory>> GetTheoryLookupAsync(IReadOnlySet<Guid> theoryIds)
    {
        if (theoryIds.Count == 0)
            return [];

        var theoryResult = await _unitOfWork.Theories.GetAllAsync(
            filter: t => theoryIds.Contains(t.TheoryID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return theoryResult.Data.ToDictionary(x => x.TheoryID);
    }

    private async Task<Dictionary<Guid, Quiz>> GetQuizLookupAsync(IReadOnlySet<Guid> quizIds)
    {
        if (quizIds.Count == 0)
            return [];

        var quizResult = await _unitOfWork.Quizs.GetAllAsync(
            filter: q => quizIds.Contains(q.QuizID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return quizResult.Data.ToDictionary(x => x.QuizID);
    }

    private async Task<Dictionary<Guid, Lab>> GetLabLookupAsync(IReadOnlySet<Guid> labIds)
    {
        if (labIds.Count == 0)
            return [];

        var labResult = await _unitOfWork.Labs.GetAllAsync(
            filter: l => labIds.Contains(l.LabID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return labResult.Data.ToDictionary(x => x.LabID);
    }

    private void PopulateReferenceInfo(Lesson lesson, LessonClientViewDTO dto, ReferenceLookups lookups)
    {
        if (lesson.ReferenceID == Guid.Empty)
            return;

        switch (lesson.Type)
        {
            case LessonType.THEORY:
                if (lookups.Theories.TryGetValue(lesson.ReferenceID, out var theory))
                {
                    MapTheoryToDto(theory, dto);
                }
                break;
            case LessonType.QUIZ:
                if (lookups.Quizs.TryGetValue(lesson.ReferenceID, out var quiz))
                {
                    MapQuizToDto(quiz, dto);
                }
                break;
            case LessonType.LAB:
                if (lookups.Labs.TryGetValue(lesson.ReferenceID, out var lab))
                {
                    MapLabToDto(lab, dto);
                }
                break;
            case LessonType.WEB:
                if (lookups.WebSimulators.TryGetValue(lesson.ReferenceID, out var webSimulator))
                {
                    MapWebSimulatorToDto(webSimulator, dto);
                }
                break;
            case LessonType.VR:
                if (lookups.VRSimulators.TryGetValue(lesson.ReferenceID, out var vrSimulator))
                {
                    MapVRSimulatorToDto(vrSimulator, dto);
                }
                break;
        }
    }

    private void MapTheoryToDto(Theory theory, LessonClientViewDTO dto)
    {
        _mapper.Map(theory, dto);
    }

    private void MapQuizToDto(Quiz quiz, LessonClientViewDTO dto)
    {
        _mapper.Map(quiz, dto);
    }

    private void MapLabToDto(Lab lab, LessonClientViewDTO dto)
    {
        _mapper.Map(lab, dto);
    }

    private void MapWebSimulatorToDto(WebSimulator webSimulator, LessonClientViewDTO dto)
    {
        _mapper.Map(webSimulator, dto);
    }

    private void MapVRSimulatorToDto(VRSimulator vrSimulator, LessonClientViewDTO dto)
    {
        _mapper.Map(vrSimulator, dto);
    }

    private sealed record ReferenceLookups(
        IReadOnlyDictionary<Guid, Theory> Theories,
        IReadOnlyDictionary<Guid, Quiz> Quizs,
        IReadOnlyDictionary<Guid, Lab> Labs,
        IReadOnlyDictionary<Guid, WebSimulator> WebSimulators,
        IReadOnlyDictionary<Guid, VRSimulator> VRSimulators);

    private async Task<Dictionary<Guid, WebSimulator>> GetWebSimulatorLookupAsync(IReadOnlySet<Guid> webSimulatorIds)
    {
        if (webSimulatorIds.Count == 0)
            return [];

        var result = await _unitOfWork.WebSimulators.GetAllAsync(
            filter: x => webSimulatorIds.Contains(x.WebSimulatorID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return result.Data.ToDictionary(x => x.WebSimulatorID);
    }

    private async Task<Dictionary<Guid, VRSimulator>> GetVRSimulatorLookupAsync(IReadOnlySet<Guid> vrSimulatorIds)
    {
        if (vrSimulatorIds.Count == 0)
            return [];

        var result = await _unitOfWork.VRSimulators.GetAllAsync(
            filter: x => vrSimulatorIds.Contains(x.VRSimulatorID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return result.Data.ToDictionary(x => x.VRSimulatorID);
    }

    public async Task DeleteLessonAsync(Guid moduleId, Guid lessonId)
    {
        var lesson = await GetLessonAsync(moduleId, lessonId);
        var deletedOrderIndex = lesson.OrderIndex;

        if (lesson.ReferenceID != Guid.Empty)
        {
            switch (lesson.Type)
            {
                case LessonType.THEORY:
                    var theory = await _unitOfWork.Theories.GetByIdAsync(lesson.ReferenceID);
                    if (theory != null)
                    {
                        await _unitOfWork.Theories.DeleteAsync(theory);
                    }
                    break;

                case LessonType.QUIZ:
                    var quiz = await _unitOfWork.Quizs.GetByIdAsync(lesson.ReferenceID);
                    if (quiz != null)
                    {
                        var questions = await _unitOfWork.QuizQuestions.GetAllAsync(
                            filter: q => q.QuizID == quiz.QuizID,
                            pageIndex: 1,
                            pageSize: int.MaxValue);

                        foreach (var question in questions.Data)
                        {
                            await _unitOfWork.QuizQuestions.DeleteAsync(question);
                        }

                        await _unitOfWork.Quizs.DeleteAsync(quiz);
                    }
                    break;
                case LessonType.WEB:
                    var webSimulator = await _unitOfWork.WebSimulators.GetByIdAsync(lesson.ReferenceID);
                    if (webSimulator != null)
                    {
                        await _unitOfWork.WebSimulators.DeleteAsync(webSimulator);
                    }
                    break;
                case LessonType.VR:
                    var vrSimulator = await _unitOfWork.VRSimulators.GetByIdAsync(lesson.ReferenceID);
                    if (vrSimulator != null)
                    {
                        await _unitOfWork.VRSimulators.DeleteAsync(vrSimulator);
                    }
                    break;
            }
        }

        await _unitOfWork.Lessons.DeleteAsync(lesson);

        var lessonsAfterDeleted = await _unitOfWork.Lessons.GetAllAsync(
            filter: l => l.ModuleID == moduleId && l.OrderIndex > deletedOrderIndex,
            orderBy: q => q.OrderBy(l => l.OrderIndex),
            pageIndex: 1,
            pageSize: int.MaxValue);

        foreach (var item in lessonsAfterDeleted.Data)
        {
            item.OrderIndex--;
            await _unitOfWork.Lessons.UpdateAsync(item);
        }

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
                var lab = await _unitOfWork.Labs.GetByIdAsync(referenceId);
                if (lab == null)
                    throw new ValidationException("Không tìm thấy tham chiếu bài lab.");

                if (lab.Status != LabStatus.ACTIVE)
                    throw new ValidationException("Chỉ có thể thêm bài lab ở trạng thái Active vào lesson.");
                break;
            case LessonType.WEB:
                if (await _unitOfWork.WebSimulators.GetByIdAsync(referenceId) == null)
                    throw new ValidationException("Không tìm thấy tham chiếu web simulator.");
                break;
            case LessonType.VR:
                if (await _unitOfWork.VRSimulators.GetByIdAsync(referenceId) == null)
                    throw new ValidationException("Không tìm thấy tham chiếu vr simulator.");
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
