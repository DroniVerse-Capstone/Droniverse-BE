using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.Helpers;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class LearningService : ILearningService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public LearningService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IClock clock, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
        _mapper = mapper;
    }

    public async Task<LearningPathDTO> GetMyLearningPathAsync(Guid enrollmentId)
    {
        var enrollment = await GetEnrollmentAsync(enrollmentId);
        var courseVersion = await GetCourseVersionAsync(enrollment.CourseVersionID);

        var modules = await GetModulesByCourseVersionAsync(enrollment.CourseVersionID);
        var moduleIds = modules.Select(x => x.ModuleID).ToArray();

        var lessons = await GetLessonsByModuleIdsAsync(moduleIds);
        var lessonIds = lessons.Select(x => x.LessonID).ToArray();

        var userLessons = await GetUserLessonsLookupAsync(_currentUser.UserId, lessonIds);
        var userModules = await GetUserModulesLookupAsync(_currentUser.UserId, moduleIds);
        var lessonMetadataLookup = await GetLessonMetadataLookupAsync(lessons);

        return BuildLearningPath(enrollment, courseVersion, modules, lessons, userLessons, userModules, lessonMetadataLookup);
    }

    public async Task ValidateLessonAccessAsync(Guid enrollmentId, Guid lessonId)
    {
        var enrollment = await GetEnrollmentAsync(enrollmentId);
        var lesson = LearningValidator.EnsureLessonExists(await _unitOfWork.Lessons.GetByIdAsync(lessonId));

        var modules = await GetModulesByCourseVersionAsync(enrollment.CourseVersionID);
        var moduleIds = modules.Select(x => x.ModuleID).ToArray();

        LearningValidator.EnsureLessonInCourseVersion(lesson, moduleIds);

        var lessons = await GetLessonsByModuleIdsAsync(moduleIds);
        var lessonIds = lessons.Select(x => x.LessonID).ToArray();
        var userLessons = await GetUserLessonsLookupAsync(_currentUser.UserId, lessonIds);

        var isLocked = IsLessonLocked(modules, lessons, userLessons, lessonId);
        LearningValidator.EnsureLessonAccessible(isLocked);
    }

    public async Task<CompleteLessonResultDTO> CompleteLessonAsync(Guid enrollmentId, Guid lessonId)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var now = _clock.Now;
            var context = await BuildCompletionContextAsync(enrollmentId, lessonId);

            EnsureTheoryLesson(context.Lesson);

            EnsureLessonAccessible(context);
            var isAlreadyCompleted = await UpsertCompletedUserLessonAsync(context, now);

            await UpdateUserModulesProgressAsync(context, now);
            await UpdateEnrollmentProgressAsync(context.Enrollment, context.Modules, context.UserModules, now);

            var certificateIssued = await TryIssueCertificateAsync(context.Enrollment, now);

            await _unitOfWork.SaveChangesAsync();

            return BuildCompleteLessonResult(context, isAlreadyCompleted, certificateIssued);
        });
    }

    private static void EnsureTheoryLesson(Lesson lesson)
    {
        if (lesson.Type != LessonType.THEORY)
            throw new ForbiddenException("Chỉ lesson lý thuyết mới có thể hoàn thành trực tiếp.");
    }

    private async Task<CompletionContext> BuildCompletionContextAsync(Guid enrollmentId, Guid lessonId)
    {
        var enrollment = await GetEnrollmentAsync(enrollmentId);
        var lesson = LearningValidator.EnsureLessonExists(await _unitOfWork.Lessons.GetByIdAsync(lessonId));

        var modules = await GetModulesByCourseVersionAsync(enrollment.CourseVersionID);
        var moduleIds = modules.Select(x => x.ModuleID).ToArray();
        LearningValidator.EnsureLessonInCourseVersion(lesson, moduleIds);

        var lessons = await GetLessonsByModuleIdsAsync(moduleIds);
        var lessonIds = lessons.Select(x => x.LessonID).ToArray();

        var userLessons = await GetUserLessonsLookupAsync(_currentUser.UserId, lessonIds);
        var userModules = await GetUserModulesLookupAsync(_currentUser.UserId, moduleIds);

        return new CompletionContext(enrollment, lesson, modules, lessons, userLessons, userModules);
    }

    private void EnsureLessonAccessible(CompletionContext context)
    {
        var isLocked = IsLessonLocked(context.Modules, context.Lessons, context.UserLessons, context.Lesson.LessonID);
        LearningValidator.EnsureLessonAccessible(isLocked);
    }

    private async Task<bool> UpsertCompletedUserLessonAsync(CompletionContext context, DateTime now)
    {
        if (context.UserLessons.TryGetValue(context.Lesson.LessonID, out var existingUserLesson))
        {
            var isAlreadyCompleted = IsCompletedUserLesson(existingUserLesson);
            existingUserLesson.Status = UserLessonStatus.COMPLETED;
            existingUserLesson.Progress = 100;
            existingUserLesson.LastAccessDate = now;
            await _unitOfWork.UserLessons.UpdateAsync(existingUserLesson);
            return isAlreadyCompleted;
        }

        var newUserLesson = new UserLesson
        {
            UserLessonID = Guid.NewGuid(),
            UserID = _currentUser.UserId,
            LessonID = context.Lesson.LessonID,
            Status = UserLessonStatus.COMPLETED,
            Progress = 100,
            LastAccessDate = now
        };

        context.UserLessons[context.Lesson.LessonID] = newUserLesson;
        await _unitOfWork.UserLessons.AddAsync(newUserLesson);
        return false;
    }

    private async Task UpdateUserModulesProgressAsync(CompletionContext context, DateTime now)
    {
        var lessonsByModule = BuildLessonsByModule(context.Lessons);
        var newUserModules = new List<UserModule>();

        foreach (var module in context.Modules)
        {
            var (moduleProgress, moduleCompleted) = CalculateModuleState(module.ModuleID, lessonsByModule, context.UserLessons);

            if (context.UserModules.TryGetValue(module.ModuleID, out var userModule))
            {
                ApplyModuleState(userModule, moduleProgress, moduleCompleted, now);
                await _unitOfWork.UserModules.UpdateAsync(userModule);
                continue;
            }

            var newUserModule = CreateUserModule(module.ModuleID, moduleProgress, moduleCompleted, now);
            context.UserModules[module.ModuleID] = newUserModule;
            newUserModules.Add(newUserModule);
        }

        if (newUserModules.Count > 0)
        {
            await _unitOfWork.UserModules.AddRangeAsync(newUserModules);
        }
    }

    private async Task UpdateEnrollmentProgressAsync(
        Enrollment enrollment,
        IReadOnlyCollection<Module> modules,
        IReadOnlyDictionary<Guid, UserModule> userModules,
        DateTime now)
    {
        var completedModules = modules.Count(module =>
            userModules.TryGetValue(module.ModuleID, out var userModule) && userModule.IsCompleted);

        enrollment.Progress = ProgressHelper.CalculateProgress(completedModules, modules.Count);
        enrollment.LastAccessDate = now;
        enrollment.Status = enrollment.Progress >= 100 ? EnrollStatus.COMPLETED : EnrollStatus.ACTIVE;

        await _unitOfWork.Enrollments.UpdateAsync(enrollment);
    }

    private static bool IsCompletedUserLesson(UserLesson userLesson)
    {
        return userLesson.Status == UserLessonStatus.COMPLETED && userLesson.Progress >= 100;
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

    private UserModule CreateUserModule(Guid moduleId, float progress, bool isCompleted, DateTime now)
    {
        return new UserModule
        {
            UserID = _currentUser.UserId,
            ModuleID = moduleId,
            EnrollDate = now,
            Progress = progress,
            IsCompleted = isCompleted,
            CompleteDate = isCompleted ? now : null
        };
    }

    private async Task<bool> TryIssueCertificateAsync(Enrollment enrollment, DateTime now)
    {
        if (enrollment.Progress < 100)
            return false;

        return await IssueCertificateIfNeededAsync(enrollment.CourseVersionID, _currentUser.UserId, now);
    }

    private static CompleteLessonResultDTO BuildCompleteLessonResult(
        CompletionContext context,
        bool isAlreadyCompleted,
        bool certificateIssued)
    {
        var currentUserModule = context.UserModules[context.Lesson.ModuleID];

        return new CompleteLessonResultDTO
        {
            EnrollmentID = context.Enrollment.EnrollmentID,
            ModuleID = context.Lesson.ModuleID,
            LessonID = context.Lesson.LessonID,
            IsAlreadyCompleted = isAlreadyCompleted,
            ModuleProgress = currentUserModule.Progress,
            IsModuleCompleted = currentUserModule.IsCompleted,
            EnrollmentProgress = context.Enrollment.Progress,
            IsEnrollmentCompleted = context.Enrollment.Status == EnrollStatus.COMPLETED,
            IsCertificateIssued = certificateIssued
        };
    }

    private async Task<Enrollment> GetEnrollmentAsync(Guid enrollmentId)
    {
        var enrollment = await _unitOfWork.Enrollments.GetByConditionAsync(
            x => x.EnrollmentID == enrollmentId && x.UserID == _currentUser.UserId);

        return LearningValidator.EnsureEnrollmentOwnedByUser(enrollment);
    }

    private async Task<CourseVersion> GetCourseVersionAsync(Guid courseVersionId)
    {
        var courseVersion = await _unitOfWork.CourseVersions.GetByIdAsync(courseVersionId);
        if (courseVersion == null)
            throw new NotFoundException("Không tìm thấy phiên bản khóa học.");

        return courseVersion;
    }

    private async Task<List<Module>> GetModulesByCourseVersionAsync(Guid courseVersionId)
    {
        var modulesResult = await _unitOfWork.Modules.GetAllAsync(
            filter: x => x.CourseVersionID == courseVersionId,
            orderBy: q => q.OrderBy(x => x.ModuleNumber),
            pageIndex: 1,
            pageSize: 10000);

        return modulesResult.Data.ToList();
    }

    private async Task<List<Lesson>> GetLessonsByModuleIdsAsync(IReadOnlyCollection<Guid> moduleIds)
    {
        if (moduleIds.Count == 0)
            return [];

        var lessonsResult = await _unitOfWork.Lessons.GetAllAsync(
            filter: x => moduleIds.Contains(x.ModuleID),
            orderBy: q => q.OrderBy(x => x.ModuleID).ThenBy(x => x.OrderIndex),
            pageIndex: 1,
            pageSize: 10000);

        return lessonsResult.Data.ToList();
    }

    private async Task<Dictionary<Guid, UserLesson>> GetUserLessonsLookupAsync(Guid userId, IReadOnlyCollection<Guid> lessonIds)
    {
        if (lessonIds.Count == 0)
            return [];

        var userLessonsResult = await _unitOfWork.UserLessons.GetAllAsync(
            filter: x => x.UserID == userId && lessonIds.Contains(x.LessonID),
            orderBy: q => q.OrderByDescending(x => x.LastAccessDate),
            pageIndex: 1,
            pageSize: 10000);

        return userLessonsResult.Data
            .GroupBy(x => x.LessonID)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.LastAccessDate).First());
    }

    private async Task<Dictionary<Guid, UserModule>> GetUserModulesLookupAsync(Guid userId, IReadOnlyCollection<Guid> moduleIds)
    {
        if (moduleIds.Count == 0)
            return [];

        var userModulesResult = await _unitOfWork.UserModules.GetAllAsync(
            filter: x => x.UserID == userId && moduleIds.Contains(x.ModuleID),
            pageIndex: 1,
            pageSize: 10000);

        return userModulesResult.Data
            .GroupBy(x => x.ModuleID)
            .ToDictionary(x => x.Key, x => x.First());
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
        var lessonsByModule = lessons
            .GroupBy(x => x.ModuleID)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.OrderIndex).ToList());

        var moduleCompletionStates = modules
            .OrderBy(x => x.ModuleNumber)
            .ToDictionary(x => x.ModuleID, _ => false);

        var moduleDtos = new List<LearningPathModuleDTO>();
        var previousModuleCompleted = true;

        foreach (var module in modules.OrderBy(x => x.ModuleNumber))
        {
            var moduleLessons = lessonsByModule.TryGetValue(module.ModuleID, out var list)
                ? list
                : [];

            var moduleLocked = UnlockHelper.IsModuleLocked(previousModuleCompleted);
            var lessonDtos = new List<LearningPathLessonDTO>();

            var previousLessonCompleted = true;
            foreach (var lesson in moduleLessons)
            {
                userLessons.TryGetValue(lesson.LessonID, out var userLesson);
                var isCompleted = userLesson is { Status: UserLessonStatus.COMPLETED } && userLesson.Progress >= 100;
                var progress = userLesson?.Progress ?? 0;
                var lessonLocked = UnlockHelper.IsLessonLocked(moduleLocked, previousLessonCompleted);

                var lessonDto = _mapper.Map<LearningPathLessonDTO>(lesson);
                if (lessonMetadataLookup.TryGetValue(lesson.LessonID, out var lessonMetadata))
                {
                    lessonDto.TitleVN = lessonMetadata.TitleVN;
                    lessonDto.TitleEN = lessonMetadata.TitleEN;
                    lessonDto.Duration = lessonMetadata.Duration;
                }
                lessonDto.IsCompleted = isCompleted;
                lessonDto.Progress = progress;
                lessonDto.IsLocked = lessonLocked;
                lessonDto.LastAccessDate = userLesson?.LastAccessDate;
                lessonDtos.Add(lessonDto);

                previousLessonCompleted = isCompleted;
            }

            var completedLessons = lessonDtos.Count(x => x.IsCompleted);
            var moduleProgress = ProgressHelper.CalculateProgress(completedLessons, lessonDtos.Count);
            var moduleCompleted = lessonDtos.Count > 0 && completedLessons == lessonDtos.Count;

            moduleCompletionStates[module.ModuleID] = moduleCompleted;
            previousModuleCompleted = moduleCompleted;

            var moduleDto = _mapper.Map<LearningPathModuleDTO>(module);
            moduleDto.Progress = userModules.TryGetValue(module.ModuleID, out var userModule)
                ? userModule.Progress
                : moduleProgress;
            moduleDto.IsCompleted = userModules.TryGetValue(module.ModuleID, out var currentUserModule)
                ? currentUserModule.IsCompleted
                : moduleCompleted;
            moduleDto.IsLocked = moduleLocked;
            moduleDto.Lessons = lessonDtos;
            moduleDto.TotalLessons = lessonDtos.Count;
            moduleDto.Duration = SumDuration(lessonDtos.Select(x => x.Duration));
            moduleDtos.Add(moduleDto);
        }

        var completedModules = moduleDtos.Count(x => x.IsCompleted);
        var enrollmentProgress = ProgressHelper.CalculateProgress(completedModules, moduleDtos.Count);

        var response = _mapper.Map<LearningPathDTO>(enrollment);
        response.TitleVN = courseVersion.TitleVN;
        response.TitleEN = courseVersion.TitleEN;
        response.TotalLessons = moduleDtos.Sum(x => x.TotalLessons);
        response.Duration = courseVersion.EstimatedDuration ?? SumDuration(moduleDtos.Select(x => x.Duration));
        response.Progress = enrollmentProgress;
        response.Modules = moduleDtos;
        return response;
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

    private static int? SumDuration(IEnumerable<int?> durations)
    {
        var values = durations
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToList();

        return values.Count == 0 ? null : values.Sum();
    }

    private bool IsLessonLocked(
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
                && userLesson.Status == UserLessonStatus.COMPLETED
                && userLesson.Progress >= 100);
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
            && userLessonState.Status == UserLessonStatus.COMPLETED
            && userLessonState.Progress >= 100;

        return UnlockHelper.IsLessonLocked(false, previousLessonCompleted);
    }

    private async Task<bool> IssueCertificateIfNeededAsync(Guid courseVersionId, Guid userId, DateTime now)
    {
        var certificate = await _unitOfWork.Certificates.GetByConditionAsync(x => x.CourseVersionID == courseVersionId);
        if (certificate == null)
            return false;

        var existing = await _unitOfWork.UserCertificates.GetByConditionAsync(
            x => x.UserID == userId && x.CertificateID == certificate.CertificateID);

        if (existing != null)
            return false;

        var userCertificate = new UserCertificate
        {
            UserID = userId,
            CertificateID = certificate.CertificateID,
            SerialNumber = Guid.NewGuid(),
            AchievedDate = now,
            Status = UserCertificateStatus.ACHIEVED
        };

        await _unitOfWork.UserCertificates.AddAsync(userCertificate);
        return true;
    }

    private sealed record CompletionContext(
        Enrollment Enrollment,
        Lesson Lesson,
        IReadOnlyCollection<Module> Modules,
        IReadOnlyCollection<Lesson> Lessons,
        Dictionary<Guid, UserLesson> UserLessons,
        Dictionary<Guid, UserModule> UserModules);

    private sealed record LessonMetadata(
        string? TitleVN,
        string? TitleEN,
        int? Duration);
}
