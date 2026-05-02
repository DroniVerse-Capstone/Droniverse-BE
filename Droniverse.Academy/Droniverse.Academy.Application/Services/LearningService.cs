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
    private readonly LearningContextLoader _learningContextLoader;
    private readonly LearningProgressService _learningProgressService;
    private readonly LearningPathAssembler _learningPathAssembler;
    private readonly LearningCertificateService _learningCertificateService;
    private readonly IUserLevelService _userLevelService;

    public LearningService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IClock clock,
        IMapper mapper,
        LearningContextLoader learningContextLoader,
        LearningProgressService learningProgressService,
        LearningPathAssembler learningPathAssembler,
        LearningCertificateService learningCertificateService,
        IUserLevelService userLevelService)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
        _mapper = mapper;
        _learningContextLoader = learningContextLoader;
        _learningProgressService = learningProgressService;
        _learningPathAssembler = learningPathAssembler;
        _learningCertificateService = learningCertificateService;
        _userLevelService = userLevelService;
    }

    public async Task<LearningPathDTO> GetMyLearningPathAsync(Guid enrollmentId)
    {
        var enrollment = await _learningContextLoader.GetEnrollmentAsync(enrollmentId, _currentUser.UserId);
        var courseVersion = await _learningContextLoader.GetCourseVersionAsync(enrollment.CourseVersionID);

        var modules = await _learningContextLoader.GetModulesByCourseVersionAsync(enrollment.CourseVersionID);
        var moduleIds = modules.Select(x => x.ModuleID).ToArray();

        var lessons = await _learningContextLoader.GetLessonsByModuleIdsAsync(moduleIds);
        var lessonIds = lessons.Select(x => x.LessonID).ToArray();

        var userLessons = await _learningContextLoader.GetUserLessonsLookupAsync(_currentUser.UserId, lessonIds);
        var userModules = await _learningContextLoader.GetUserModulesLookupAsync(_currentUser.UserId, moduleIds);

        return await _learningPathAssembler.BuildLearningPathAsync(
            enrollment,
            courseVersion,
            modules,
            lessons,
            userLessons,
            userModules);
    }

    public async Task<LearningPathDTO> GetLearningPathAsync(Guid enrollmentId)
    {
        var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId)
            ?? throw new NotFoundException("Không tìm thấy enrollment.");

        var courseVersion = await _learningContextLoader.GetCourseVersionAsync(enrollment.CourseVersionID);

        var modules = await _learningContextLoader.GetModulesByCourseVersionAsync(enrollment.CourseVersionID);
        var moduleIds = modules.Select(x => x.ModuleID).ToArray();

        var lessons = await _learningContextLoader.GetLessonsByModuleIdsAsync(moduleIds);
        var lessonIds = lessons.Select(x => x.LessonID).ToArray();

        var userLessons = await _learningContextLoader.GetUserLessonsLookupAsync(enrollment.UserID, lessonIds);
        var userModules = await _learningContextLoader.GetUserModulesLookupAsync(enrollment.UserID, moduleIds);

        return await _learningPathAssembler.BuildLearningPathAsync(
            enrollment,
            courseVersion,
            modules,
            lessons,
            userLessons,
            userModules);
    }

    public async Task<IEnumerable<IncompleteVRLessonResponseDTO>> GetListVRsAsync()
    {
        var userLessonsResult = await _unitOfWork.UserLessons.GetAllAsync(
            filter: x => x.UserID == _currentUser.UserId
                         && (x.Status == UserLessonStatus.COMPLETED
                             || x.Status == UserLessonStatus.INCOMPLETED)
                         && x.Lesson.Type == LessonType.VR,
            orderBy: q => q.OrderByDescending(x => x.LastAccessDate),
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "Lesson.Module");

        var userLessons = userLessonsResult.Data.ToList();
        if (userLessons.Count == 0)
            return [];

        var vrReferenceIds = userLessons
            .Select(x => x.Lesson.ReferenceID)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        var vrLookup = new Dictionary<Guid, VRSimulator>();
        if (vrReferenceIds.Length > 0)
        {
            var vrResult = await _unitOfWork.VRSimulators.GetAllAsync(
                filter: x => vrReferenceIds.Contains(x.VRSimulatorID),
                pageIndex: 1,
                pageSize: int.MaxValue);

            vrLookup = vrResult.Data.ToDictionary(x => x.VRSimulatorID);
        }
        var courseVersionIds = userLessons
            .Where(x => x.Lesson?.Module != null)
            .Select(x => x.Lesson.Module.CourseVersionID)
            .Distinct()
            .ToList();

        var enrollmentsResult = await _unitOfWork.Enrollments.GetAllAsync(
            filter: x =>
                x.UserID == _currentUser.UserId &&
                courseVersionIds.Contains(x.CourseVersionID) &&
                x.Status == EnrollStatus.ACTIVE,
            pageIndex: 1,
            pageSize: int.MaxValue);

        var enrollmentLookup = enrollmentsResult.Data.ToDictionary(x => x.CourseVersionID);
        return userLessons.Select(userLesson =>
        {
            var lesson = userLesson.Lesson;
            var module = lesson?.Module;
            VRSimulator? vrSimulator = null;
            if (lesson != null)
            {
                vrLookup.TryGetValue(lesson.ReferenceID, out vrSimulator);
            }

            if (lesson == null || module == null)
                return new IncompleteVRLessonResponseDTO
                {
                    UserLessonID = userLesson.UserLessonID,
                    LessonID = lesson?.LessonID ?? Guid.Empty,
                    ModuleID = lesson?.ModuleID ?? Guid.Empty,
                    OrderIndex = lesson?.OrderIndex ?? 0,
                    ReferenceID = lesson?.ReferenceID ?? Guid.Empty,
                    Type = lesson?.Type ?? LessonType.VR,
                    Status = userLesson.Status,
                    Progress = userLesson.Progress,
                    LastAccessDate = userLesson.LastAccessDate,
                    EnrollmentID = Guid.Empty
                };

            enrollmentLookup.TryGetValue(module.CourseVersionID, out var enrollment);
            var enrollmentId = enrollment?.EnrollmentID ?? Guid.Empty;

            return new IncompleteVRLessonResponseDTO
            {
                UserLessonID = userLesson.UserLessonID,
                LessonID = lesson.LessonID,
                ModuleID = lesson.ModuleID,
                OrderIndex = lesson.OrderIndex,
                ReferenceID = lesson.ReferenceID,
                Type = lesson.Type,
                TitleVN = vrSimulator?.TitleVN,
                TitleEN = vrSimulator?.TitleEN,
                EstimatedTime = vrSimulator?.EstimatedTime,
                Status = userLesson.Status,
                Progress = userLesson.Progress,
                LastAccessDate = userLesson.LastAccessDate,
                EnrollmentID = enrollmentId
            };
        });
    }

    public async Task<UserLessonResponseDTO> CreateUserLessonAsync(Guid enrollmentId, Guid lessonId)
    {
        await ValidateLessonAccessAsync(enrollmentId, lessonId);

        var userLesson = await _unitOfWork.UserLessons.GetByConditionAsync(
            x => x.UserID == _currentUser.UserId && x.LessonID == lessonId);

        if (userLesson != null)
            throw new ValidationException("Người dùng đã có dữ liệu lesson này.");

        userLesson = new UserLesson
        {
            UserLessonID = Guid.NewGuid(),
            UserID = _currentUser.UserId,
            LessonID = lessonId,
            LastAccessDate = _clock.Now
        };
        userLesson.SetProgress(0);

        await _unitOfWork.UserLessons.AddAsync(userLesson);

        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<UserLessonResponseDTO>(userLesson);
    }

    public async Task<bool> CheckUserLessonExistsAsync(Guid enrollmentId, Guid lessonId)
    {
        var enrollment = await _learningContextLoader.GetEnrollmentAsync(enrollmentId, _currentUser.UserId);
        var lesson = LearningValidator.EnsureLessonExists(await _unitOfWork.Lessons.GetByIdAsync(lessonId));

        var modules = await _learningContextLoader.GetModulesByCourseVersionAsync(enrollment.CourseVersionID);
        var moduleIds = modules.Select(x => x.ModuleID).ToArray();
        LearningValidator.EnsureLessonInCourseVersion(lesson, moduleIds);

        var userLesson = await _unitOfWork.UserLessons.GetByConditionAsync(
            x => x.UserID == _currentUser.UserId && x.LessonID == lessonId);

        return userLesson != null;
    }

    public async Task<UserModuleResponseDTO> GetOrCreateUserModuleAsync(Guid enrollmentId, Guid moduleId)
    {
        var enrollment = await _learningContextLoader.GetEnrollmentAsync(enrollmentId, _currentUser.UserId);
        var modules = await _learningContextLoader.GetModulesByCourseVersionAsync(enrollment.CourseVersionID);

        var targetModule = modules.FirstOrDefault(x => x.ModuleID == moduleId)
            ?? throw new NotFoundException("Không tìm thấy module.");

        var moduleIds = modules.Select(x => x.ModuleID).ToArray();
        var userModules = await _learningContextLoader.GetUserModulesLookupAsync(_currentUser.UserId, moduleIds);

        var isLocked = _learningProgressService.IsModuleLocked(modules, userModules, targetModule.ModuleID);
        if (isLocked)
            throw new ForbiddenException("Module chưa được mở. Vui lòng hoàn thành module trước đó.");

        var userModule = await _unitOfWork.UserModules.GetByConditionAsync(
            x => x.UserID == _currentUser.UserId && x.ModuleID == moduleId);

        if (userModule == null)
        {
            userModule = new UserModule
            {
                UserModuleID = Guid.NewGuid(),
                UserID = _currentUser.UserId,
                ModuleID = moduleId,
                EnrollDate = _clock.Now,
                Progress = 0,
                IsCompleted = false,
                CompleteDate = null
            };

            await _unitOfWork.UserModules.AddAsync(userModule);
        }

        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<UserModuleResponseDTO>(userModule);
    }

    public async Task ValidateLessonAccessAsync(Guid enrollmentId, Guid lessonId)
    {
        var enrollment = await _learningContextLoader.GetEnrollmentAsync(enrollmentId, _currentUser.UserId);
        var lesson = LearningValidator.EnsureLessonExists(await _unitOfWork.Lessons.GetByIdAsync(lessonId));

        var modules = await _learningContextLoader.GetModulesByCourseVersionAsync(enrollment.CourseVersionID);
        var moduleIds = modules.Select(x => x.ModuleID).ToArray();

        var lessons = await _learningContextLoader.GetLessonsByModuleIdsAsync(moduleIds);
        var lessonIds = lessons.Select(x => x.LessonID).ToArray();
        var userLessons = await _learningContextLoader.GetUserLessonsLookupAsync(_currentUser.UserId, lessonIds);

        _learningProgressService.ValidateLessonAccess(lesson, moduleIds, modules, lessons, userLessons);
    }

    public async Task<CompleteLessonResultDTO> CompleteLessonAsync(Guid enrollmentId, Guid lessonId)
    {
        return await CompleteLessonInternalAsync(enrollmentId, lessonId, CompletionMode.Direct);
    }

    public async Task<CompleteLessonResultDTO> CompleteLessonBySimulatorSubmitAsync(Guid enrollmentId, Guid lessonId)
    {
        return await CompleteLessonInternalAsync(enrollmentId, lessonId, CompletionMode.SimulatorSubmit);
    }

    public async Task<CompleteLessonResultDTO> CompleteLessonByAssessmentAsync(Guid enrollmentId, Guid lessonId)
    {
        return await CompleteLessonInternalAsync(enrollmentId, lessonId, CompletionMode.Assessment);
    }

    private async Task<CompleteLessonResultDTO> CompleteLessonInternalAsync(Guid enrollmentId, Guid lessonId, CompletionMode mode)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var now = _clock.Now;
            var context = await BuildCompletionContextAsync(enrollmentId, lessonId);

            EnsureLessonCanBeCompletedInMode(context.Lesson, mode);

            _learningProgressService.ValidateLessonAccess(
                context.Lesson,
                context.Modules.Select(x => x.ModuleID).ToArray(),
                context.Modules,
                context.Lessons,
                context.UserLessons);

            var isAlreadyCompleted = await UpsertCompletedUserLessonAsync(context, now);

            await _learningProgressService.UpdateUserModulesProgressAsync(
                context.Modules,
                context.Lessons,
                context.UserLessons,
                context.UserModules,
                moduleId => CreateUserModule(moduleId, now),
                now);

            await _learningProgressService.UpdateEnrollmentProgressAsync(
                context.Enrollment,
                context.Modules,
                context.Lessons,
                context.UserLessons,
                context.UserModules,
                now);

            await TryUpgradeUserLevelAsync(context.Enrollment);

            var certificateIssued = await _learningCertificateService.TryIssueCertificateAsync(
                context.Enrollment,
                _currentUser.UserId,
                now);

            await _unitOfWork.SaveChangesAsync();

            return BuildCompleteLessonResult(context, isAlreadyCompleted, certificateIssued);
        });
    }

    private static void EnsureLessonCanBeCompletedInMode(Lesson lesson, CompletionMode mode)
    {
        if (mode == CompletionMode.Direct && lesson.Type is not (LessonType.PHYSIC or LessonType.THEORY))
            throw new ForbiddenException("Chỉ lesson theory, physic mới có thể hoàn thành trực tiếp.");

        if (mode == CompletionMode.SimulatorSubmit && lesson.Type is not (LessonType.LAB_PHYSIC or LessonType.VR))
            throw new ForbiddenException("Chỉ lesson simulator (lab_physic hoặc VR) mới có thể hoàn thành qua nộp simulator.");

        if (mode == CompletionMode.Assessment && lesson.Type is not (LessonType.QUIZ or LessonType.LAB or LessonType.ASSIGNMENT))
            throw new ForbiddenException("Chỉ lesson quiz, lab hoặc assignment mới có thể hoàn thành qua nộp bài.");
    }

    private async Task<CompletionContext> BuildCompletionContextAsync(Guid enrollmentId, Guid lessonId)
    {
        var enrollment = await _learningContextLoader.GetEnrollmentAsync(enrollmentId, _currentUser.UserId);
        var lesson = LearningValidator.EnsureLessonExists(await _unitOfWork.Lessons.GetByIdAsync(lessonId));

        var modules = await _learningContextLoader.GetModulesByCourseVersionAsync(enrollment.CourseVersionID);
        var moduleIds = modules.Select(x => x.ModuleID).ToArray();

        var lessons = await _learningContextLoader.GetLessonsByModuleIdsAsync(moduleIds);
        var lessonIds = lessons.Select(x => x.LessonID).ToArray();

        var userLessons = await _learningContextLoader.GetUserLessonsLookupAsync(_currentUser.UserId, lessonIds);
        var userModules = await _learningContextLoader.GetUserModulesLookupAsync(_currentUser.UserId, moduleIds);

        return new CompletionContext(enrollment, lesson, modules, lessons, userLessons, userModules);
    }

    private async Task<bool> UpsertCompletedUserLessonAsync(CompletionContext context, DateTime now)
    {
        if (context.UserLessons.TryGetValue(context.Lesson.LessonID, out var existingUserLesson))
        {
            var isAlreadyCompleted = IsCompletedUserLesson(existingUserLesson);
            existingUserLesson.Complete();
            existingUserLesson.LastAccessDate = now;
            await _unitOfWork.UserLessons.UpdateAsync(existingUserLesson);
            return isAlreadyCompleted;
        }

        var newUserLesson = new UserLesson
        {
            UserLessonID = Guid.NewGuid(),
            UserID = _currentUser.UserId,
            LessonID = context.Lesson.LessonID,
            LastAccessDate = now
        };
        newUserLesson.Complete();

        context.UserLessons[context.Lesson.LessonID] = newUserLesson;
        await _unitOfWork.UserLessons.AddAsync(newUserLesson);
        return false;
    }

    private static bool IsCompletedUserLesson(UserLesson? userLesson)
    {
        if (userLesson == null)
            return false;

        return userLesson.Status == UserLessonStatus.COMPLETED;
    }


    private UserModule CreateUserModule(Guid moduleId, DateTime now)
    {
        return new UserModule
        {
            UserModuleID = Guid.NewGuid(),
            UserID = _currentUser.UserId,
            ModuleID = moduleId,
            EnrollDate = now,
            Progress = 0,
            IsCompleted = false,
            CompleteDate = null
        };
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

    private async Task TryUpgradeUserLevelAsync(Enrollment enrollment)
    {
        if (enrollment.Status != EnrollStatus.COMPLETED)
            return;

        var course = await _unitOfWork.Courses.GetByIdAsync(enrollment.CourseID);
        if (course?.DroneID == null || course.DroneID == Guid.Empty)
            return;

        var canUpgrade = await _userLevelService.CanUserUpgradeAsync(_currentUser.UserId, course.DroneID.Value);
        if (!canUpgrade)
            return;

        await _userLevelService.UpgradeUserLevelAsync(_currentUser.UserId, course.DroneID.Value);
    }


    private sealed record CompletionContext(
        Enrollment Enrollment,
        Lesson Lesson,
        IReadOnlyCollection<Module> Modules,
        IReadOnlyCollection<Lesson> Lessons,
        Dictionary<Guid, UserLesson> UserLessons,
        Dictionary<Guid, UserModule> UserModules);

    private enum CompletionMode
    {
        Direct,
        SimulatorSubmit,
        Assessment
    }
}
