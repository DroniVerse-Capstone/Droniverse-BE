using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.Helpers;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;

namespace Droniverse.Academy.Application.Services;

public sealed class LearningPathAssembler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public LearningPathAssembler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<LearningPathDTO> BuildLearningPathAsync(
        Enrollment enrollment,
        CourseVersion courseVersion,
        IReadOnlyCollection<Module> modules,
        IReadOnlyCollection<Lesson> lessons,
        IReadOnlyDictionary<Guid, UserLesson> userLessons,
        IReadOnlyDictionary<Guid, UserModule> userModules)
    {
        var lessonMetadataLookup = await GetLessonMetadataLookupAsync(lessons);
        return BuildLearningPath(enrollment, courseVersion, modules, lessons, userLessons, userModules, lessonMetadataLookup);
    }

    private LearningPathDTO BuildLearningPath(
        Enrollment enrollment,
        CourseVersion courseVersion,
        IReadOnlyCollection<Module> modules,
        IReadOnlyCollection<Lesson> lessons,
        IReadOnlyDictionary<Guid, UserLesson> userLessons,
        IReadOnlyDictionary<Guid, UserModule> userModules,
        IReadOnlyDictionary<Guid, LessonMetadata> lessonMetadataLookup)
    {
        var lessonsByModule = BuildLessonsByModule(lessons);

        var moduleDTOs = new List<LearningPathModuleDTO>();
        var previousModuleCompleted = true;

        foreach (var module in modules.OrderBy(x => x.ModuleNumber))
        {
            var moduleLessons = lessonsByModule.TryGetValue(module.ModuleID, out var list)
                ? list
                : [];

            var (moduleDTO, moduleCompleted) = BuildModuleDTO(
                module,
                moduleLessons,
                userLessons,
                userModules,
                lessonMetadataLookup,
                previousModuleCompleted);

            previousModuleCompleted = moduleCompleted;
            moduleDTOs.Add(moduleDTO);
        }

        var completedModules = moduleDTOs.Count(x => x.IsCompleted);
        var enrollmentProgress = ProgressHelper.CalculateProgress(completedModules, moduleDTOs.Count);

        var response = _mapper.Map<LearningPathDTO>(enrollment);
        response.TitleVN = courseVersion.TitleVN;
        response.TitleEN = courseVersion.TitleEN;
        response.TotalLessons = moduleDTOs.Sum(x => x.TotalLessons);
        response.Duration = courseVersion.EstimatedDuration ?? SumDuration(moduleDTOs.Select(x => x.Duration));
        response.Progress = enrollmentProgress;
        response.Modules = moduleDTOs;
        return response;
    }

    private (LearningPathModuleDTO ModuleDTO, bool IsCompleted) BuildModuleDTO(
        Module module,
        IReadOnlyCollection<Lesson> moduleLessons,
        IReadOnlyDictionary<Guid, UserLesson> userLessons,
        IReadOnlyDictionary<Guid, UserModule> userModules,
        IReadOnlyDictionary<Guid, LessonMetadata> lessonMetadataLookup,
        bool previousModuleCompleted)
    {
        var moduleLocked = UnlockHelper.IsModuleLocked(previousModuleCompleted);
        var lessonDTOs = BuildLessonDTOs(moduleLessons, userLessons, lessonMetadataLookup, moduleLocked);

        var completedLessons = lessonDTOs.Count(x => x.IsCompleted);
        var moduleProgress = ProgressHelper.CalculateProgress(completedLessons, lessonDTOs.Count);
        var moduleCompleted = lessonDTOs.Count > 0 && completedLessons == lessonDTOs.Count;

        var moduleDTO = _mapper.Map<LearningPathModuleDTO>(module);
        if (userModules.TryGetValue(module.ModuleID, out var userModule))
        {
            moduleDTO.Progress = userModule.Progress;
            moduleDTO.IsCompleted = userModule.IsCompleted;
        }
        else
        {
            moduleDTO.Progress = moduleProgress;
            moduleDTO.IsCompleted = moduleCompleted;
        }

        moduleDTO.IsLocked = moduleLocked;
        moduleDTO.Lessons = lessonDTOs;
        moduleDTO.TotalLessons = lessonDTOs.Count;
        moduleDTO.Duration = SumDuration(lessonDTOs.Select(x => x.Duration));

        return (moduleDTO, moduleCompleted);
    }

    private List<LearningPathLessonDTO> BuildLessonDTOs(
        IReadOnlyCollection<Lesson> moduleLessons,
        IReadOnlyDictionary<Guid, UserLesson> userLessons,
        IReadOnlyDictionary<Guid, LessonMetadata> lessonMetadataLookup,
        bool moduleLocked)
    {
        var lessonDTOs = new List<LearningPathLessonDTO>(moduleLessons.Count);
        var previousLessonCompleted = true;

        foreach (var lesson in moduleLessons)
        {
            userLessons.TryGetValue(lesson.LessonID, out var userLesson);
            var isCompleted = IsCompletedUserLesson(userLesson);
            var lessonLocked = UnlockHelper.IsLessonLocked(moduleLocked, previousLessonCompleted);

            var lessonDTO = _mapper.Map<LearningPathLessonDTO>(lesson);
            if (lessonMetadataLookup.TryGetValue(lesson.LessonID, out var lessonMetadata))
            {
                lessonDTO.TitleVN = lessonMetadata.TitleVN;
                lessonDTO.TitleEN = lessonMetadata.TitleEN;
                lessonDTO.Duration = lessonMetadata.Duration;
            }

            lessonDTO.IsCompleted = isCompleted;
            lessonDTO.Progress = userLesson?.Progress ?? 0;
            lessonDTO.IsLocked = lessonLocked;
            lessonDTO.LastAccessDate = userLesson?.LastAccessDate;
            lessonDTOs.Add(lessonDTO);

            previousLessonCompleted = isCompleted;
        }

        return lessonDTOs;
    }

    private async Task<Dictionary<Guid, LessonMetadata>> GetLessonMetadataLookupAsync(IReadOnlyCollection<Lesson> lessons)
    {
        if (lessons.Count == 0)
            return [];

        var theoryIds = lessons
            .Where(x => x.Type == LessonType.THEORY)
            .Select(x => x.ReferenceID)
            .ToHashSet();

        var quizIds = lessons
            .Where(x => x.Type == LessonType.QUIZ)
            .Select(x => x.ReferenceID)
            .ToHashSet();

        var labIds = lessons
            .Where(x => x.Type == LessonType.LAB)
            .Select(x => x.ReferenceID)
            .ToHashSet();

        var theoryLookup = await GetTheoryMetadataLookupAsync(theoryIds);
        var quizLookup = await GetQuizMetadataLookupAsync(quizIds);
        var labLookup = await GetLabMetadataLookupAsync(labIds);

        var result = new Dictionary<Guid, LessonMetadata>();
        foreach (var lesson in lessons)
        {
            LessonMetadata? metadata = lesson.Type switch
            {
                LessonType.THEORY => theoryLookup.GetValueOrDefault(lesson.ReferenceID),
                LessonType.QUIZ => quizLookup.GetValueOrDefault(lesson.ReferenceID),
                LessonType.LAB => labLookup.GetValueOrDefault(lesson.ReferenceID),
                _ => null
            };

            if (metadata != null)
                result[lesson.LessonID] = metadata;
        }

        return result;
    }

    private async Task<Dictionary<Guid, LessonMetadata>> GetTheoryMetadataLookupAsync(IReadOnlySet<Guid> theoryIds)
    {
        if (theoryIds.Count == 0)
            return [];

        var theories = await _unitOfWork.Theories.GetAllAsync(
            filter: x => theoryIds.Contains(x.TheoryID),
            pageIndex: 1,
            pageSize: 10000);

        return theories.Data.ToDictionary(
            x => x.TheoryID,
            x => new LessonMetadata(x.TitleVN, x.TitleEN, x.EstimatedTime));
    }

    private async Task<Dictionary<Guid, LessonMetadata>> GetQuizMetadataLookupAsync(IReadOnlySet<Guid> quizIds)
    {
        if (quizIds.Count == 0)
            return [];

        var quizzes = await _unitOfWork.Quizs.GetAllAsync(
            filter: x => quizIds.Contains(x.QuizID),
            pageIndex: 1,
            pageSize: 10000);

        return quizzes.Data.ToDictionary(
            x => x.QuizID,
            x => new LessonMetadata(x.TitleVN, x.TitleEN, x.TimeLimit));
    }

    private async Task<Dictionary<Guid, LessonMetadata>> GetLabMetadataLookupAsync(IReadOnlySet<Guid> labIds)
    {
        if (labIds.Count == 0)
            return [];

        var labs = await _unitOfWork.Labs.GetAllAsync(
            filter: x => labIds.Contains(x.LabID),
            pageIndex: 1,
            pageSize: 10000);

        return labs.Data.ToDictionary(
            x => x.LabID,
            x => new LessonMetadata(x.NameVN, x.NameEN, x.EstimatedTime));
    }

    private static bool IsCompletedUserLesson(UserLesson? userLesson)
    {
        if (userLesson == null)
            return false;

        return userLesson.Status == UserLessonStatus.COMPLETED;
    }

    private static Dictionary<Guid, List<Lesson>> BuildLessonsByModule(IReadOnlyCollection<Lesson> lessons)
    {
        return lessons
            .GroupBy(x => x.ModuleID)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.OrderIndex).ToList());
    }

    private static int? SumDuration(IEnumerable<int?> durations)
    {
        var values = durations
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        return values.Count == 0 ? null : values.Sum();
    }

    private sealed record LessonMetadata(
        string? TitleVN,
        string? TitleEN,
        int? Duration);
}
