using Droniverse.Academy.Application.Helpers;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public sealed class LearningProgressService
{
    private readonly IUnitOfWork _unitOfWork;

    public LearningProgressService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void ValidateLessonAccess(
        Lesson lesson,
        IReadOnlyCollection<Guid> moduleIds,
        IReadOnlyCollection<Module> modules,
        IReadOnlyCollection<Lesson> lessons,
        IReadOnlyDictionary<Guid, UserLesson> userLessons)
    {
        LearningValidator.EnsureLessonInCourseVersion(lesson, moduleIds);

        var isLocked = IsLessonLocked(modules, lessons, userLessons, lesson.LessonID);
        LearningValidator.EnsureLessonAccessible(isLocked);
    }

    public bool IsModuleLocked(
        IReadOnlyCollection<Module> modules,
        IReadOnlyDictionary<Guid, UserModule> userModules,
        Guid moduleId)
    {
        var orderedModules = modules.OrderBy(x => x.ModuleNumber).ToList();
        var targetIndex = orderedModules.FindIndex(x => x.ModuleID == moduleId);

        if (targetIndex < 0)
            throw new NotFoundException("Không tìm thấy module.");

        if (targetIndex == 0)
            return false;

        var previousModule = orderedModules[targetIndex - 1];
        var previousModuleCompleted = userModules.TryGetValue(previousModule.ModuleID, out var userModule)
            && userModule.IsCompleted;

        return UnlockHelper.IsModuleLocked(previousModuleCompleted);
    }

    public async Task UpdateUserModulesProgressAsync(
        IReadOnlyCollection<Module> modules,
        IReadOnlyCollection<Lesson> lessons,
        Dictionary<Guid, UserLesson> userLessons,
        Dictionary<Guid, UserModule> userModules,
        Func<Guid, UserModule> createUserModule,
        DateTime now)
    {
        var lessonsByModule = BuildLessonsByModule(lessons);
        var newUserModules = new List<UserModule>();

        foreach (var module in modules)
        {
            var (moduleProgress, moduleCompleted) = CalculateModuleState(module.ModuleID, lessonsByModule, userLessons);

            if (userModules.TryGetValue(module.ModuleID, out var userModule))
            {
                ApplyModuleState(userModule, moduleProgress, moduleCompleted, now);
                await _unitOfWork.UserModules.UpdateAsync(userModule);
                continue;
            }

            var newUserModule = createUserModule(module.ModuleID);
            ApplyModuleState(newUserModule, moduleProgress, moduleCompleted, now);
            userModules[module.ModuleID] = newUserModule;
            newUserModules.Add(newUserModule);
        }

        if (newUserModules.Count > 0)
        {
            await _unitOfWork.UserModules.AddRangeAsync(newUserModules);
        }
    }

    public async Task UpdateEnrollmentProgressAsync(
        Enrollment enrollment,
        IReadOnlyCollection<Module> modules,
        IReadOnlyCollection<Lesson> lessons,
        IReadOnlyDictionary<Guid, UserLesson> userLessons,
        IReadOnlyDictionary<Guid, UserModule> userModules,
        DateTime now)
    {
        var completedLessons = lessons.Count(lesson =>
            userLessons.TryGetValue(lesson.LessonID, out var userLesson) && IsCompletedUserLesson(userLesson));

        enrollment.Progress = ProgressHelper.CalculateProgress(completedLessons, lessons.Count);
        enrollment.LastAccessDate = now;
        enrollment.Status = enrollment.Progress >= 100 ? EnrollStatus.COMPLETED : EnrollStatus.ACTIVE;

        await _unitOfWork.Enrollments.UpdateAsync(enrollment);
    }

    private static Dictionary<Guid, List<Lesson>> BuildLessonsByModule(IReadOnlyCollection<Lesson> lessons)
    {
        return lessons
            .GroupBy(x => x.ModuleID)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.OrderIndex).ToList());
    }

    private static (float Progress, bool IsCompleted) CalculateModuleState(
        Guid moduleId,
        IReadOnlyDictionary<Guid, List<Lesson>> lessonsByModule,
        IReadOnlyDictionary<Guid, UserLesson> userLessons)
    {
        var moduleLessons = lessonsByModule.TryGetValue(moduleId, out var currentModuleLessons)
            ? currentModuleLessons
            : [];

        var completedLessons = moduleLessons.Count(x =>
            userLessons.TryGetValue(x.LessonID, out var userLesson) && IsCompletedUserLesson(userLesson));

        var moduleProgress = ProgressHelper.CalculateProgress(completedLessons, moduleLessons.Count);
        var moduleCompleted = moduleLessons.Count > 0 && completedLessons == moduleLessons.Count;
        return (moduleProgress, moduleCompleted);
    }

    private static void ApplyModuleState(UserModule userModule, float progress, bool isCompleted, DateTime now)
    {
        userModule.Progress = progress;
        userModule.IsCompleted = isCompleted;
        userModule.CompleteDate = isCompleted ? userModule.CompleteDate ?? now : null;
    }

    private static bool IsCompletedUserLesson(UserLesson? userLesson)
    {
        if (userLesson == null)
            return false;

        return userLesson.Status == UserLessonStatus.COMPLETED;
    }

    private static bool IsLessonLocked(
        IReadOnlyCollection<Module> modules,
        IReadOnlyCollection<Lesson> lessons,
        IReadOnlyDictionary<Guid, UserLesson> userLessons,
        Guid lessonId)
    {
        var targetLesson = lessons.FirstOrDefault(x => x.LessonID == lessonId);
        if (targetLesson == null)
            throw new NotFoundException("Không tìm thấy lesson.");

        var orderedModules = modules.OrderBy(x => x.ModuleNumber).ToList();
        var targetModule = orderedModules.First(x => x.ModuleID == targetLesson.ModuleID);

        var lessonsByModule = lessons
            .GroupBy(x => x.ModuleID)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.OrderIndex).ToList());

        var previousModuleIndex = orderedModules.FindIndex(x => x.ModuleID == targetModule.ModuleID) - 1;
        var previousModuleCompleted = true;

        if (previousModuleIndex >= 0)
        {
            var previousModule = orderedModules[previousModuleIndex];
            var previousLessons = lessonsByModule.TryGetValue(previousModule.ModuleID, out var value) ? value : [];
            previousModuleCompleted = previousLessons.Count > 0 && previousLessons.All(x =>
                userLessons.TryGetValue(x.LessonID, out var userLesson)
                && IsCompletedUserLesson(userLesson));
        }

        var moduleLocked = UnlockHelper.IsModuleLocked(previousModuleCompleted);
        if (moduleLocked)
            return true;

        var currentModuleLessons = lessonsByModule[targetModule.ModuleID];
        var orderedLessons = currentModuleLessons.OrderBy(x => x.OrderIndex).ToList();
        var targetLessonIndex = orderedLessons.FindIndex(x => x.LessonID == lessonId);

        if (targetLessonIndex <= 0)
            return false;

        var previousLesson = orderedLessons[targetLessonIndex - 1];
        var previousLessonCompleted = userLessons.TryGetValue(previousLesson.LessonID, out var userLessonState)
            && IsCompletedUserLesson(userLessonState);

        return UnlockHelper.IsLessonLocked(false, previousLessonCompleted);
    }
}
