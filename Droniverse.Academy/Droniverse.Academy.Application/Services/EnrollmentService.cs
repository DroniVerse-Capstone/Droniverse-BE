using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.DTO.Extension;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public EnrollmentService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<EnrollmentResponseDTO> CreateEnrollmentAsync(CreateEnrollmentRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var courseVersion = await _unitOfWork.CourseVersions.GetByIdAsync(request.CourseVersionID);
        if (courseVersion == null)
            throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        var userId = _currentUser.UserId;

        var existing = await _unitOfWork.Enrollments.GetByConditionAsync(
            x => x.UserID == userId && x.CourseVersionID == request.CourseVersionID);

        if (existing != null)
            throw new ValidationException("Người dùng đã đăng ký phiên bản khóa học này.");

        var enrollment = _mapper.Map<Enrollment>(request);
        enrollment.EnrollmentID = Guid.NewGuid();
        enrollment.CourseID = courseVersion.CourseID;
        enrollment.UserID = userId;
        enrollment.EnrollDate = _clock.Now;
        enrollment.LastAccessDate = _clock.Now;
        enrollment.ExpireDate = request.ExpireDate ?? _clock.Now.AddMonths(6);
        enrollment.Progress = 0;
        enrollment.Status = EnrollStatus.ACTIVE;

        await _unitOfWork.Enrollments.AddAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<EnrollmentResponseDTO>(enrollment);
    }

    public async Task<PaginationResult<IEnumerable<EnrollmentResponseDTO>>> GetMyEnrollmentsAsync(int pageIndex = 1, int pageSize = 10, EnrollStatus? status = null)
    {
        var userId = _currentUser.UserId;

        var result = await _unitOfWork.Enrollments.GetAllAsync(
            status.HasValue
                ? x => x.UserID == userId && x.Status == status.Value
                : x => x.UserID == userId,
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.EnrollDate));

        var mapped = result.Data.Select(x => _mapper.Map<EnrollmentResponseDTO>(x)).ToList();
        return new PaginationResult<IEnumerable<EnrollmentResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<EnrollmentResponseDTO> GetMyEnrollmentByIdAsync(Guid enrollmentId)
    {
        var enrollment = await GetMyEnrollmentEntityOrThrowAsync(enrollmentId);

        return _mapper.Map<EnrollmentResponseDTO>(enrollment);
    }

    public async Task<EnrollmentResponseDTO> UpdateMyEnrollmentAsync(Guid enrollmentId, UpdateEnrollmentRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!request.Progress.HasValue && !request.LastAccessDate.HasValue && !request.ExpireDate.HasValue && !request.Status.HasValue)
            throw new ValidationException("Cần ít nhất một trường để cập nhật enrollment.");

        if (request.Progress.HasValue && request.Progress is < 0 or > 100)
            throw new ValidationException("Progress phải nằm trong khoảng từ 0 đến 100.");

        var enrollment = await GetMyEnrollmentEntityOrThrowAsync(enrollmentId);

        if (request.Progress.HasValue)
            enrollment.Progress = request.Progress.Value;

        enrollment.LastAccessDate = request.LastAccessDate ?? _clock.Now;

        if (request.ExpireDate.HasValue)
            enrollment.ExpireDate = request.ExpireDate.Value;

        if (request.Status.HasValue)
            enrollment.Status = request.Status.Value;

        await _unitOfWork.Enrollments.UpdateAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<EnrollmentResponseDTO>(enrollment);
    }

    public async Task<EnrollmentLearningPathResponseDTO> GetMyLearningPathAsync(Guid enrollmentId)
    {
        var enrollment = await GetMyEnrollmentEntityOrThrowAsync(enrollmentId);

        var modules = await GetModules(enrollment.CourseVersionID);

        if (!modules.Any())
            return BuildEmptyResponse(enrollment);

        var lessons = await GetLessons(modules);
        var userLessons = await GetUserLessons(enrollment.UserID, lessons);

        return BuildLearningPathResponse(enrollment, modules, lessons, userLessons);
    }

    public async Task<EnrollmentNextLessonResponseDTO?> GetMyNextLessonAsync(Guid enrollmentId)
    {
        var learningPath = await GetMyLearningPathAsync(enrollmentId);

        var nextLesson = learningPath.Modules
            .OrderBy(x => x.ModuleNumber)
            .SelectMany(x => x.Lessons.OrderBy(y => y.OrderIndex), (module, lesson) => new { module, lesson })
            .FirstOrDefault(x => x.lesson.Status != UserLessonStatus.COMPLETED && (x.lesson.Progress ?? 0) < 100);

        if (nextLesson == null)
            return null;

        var response = _mapper.Map<EnrollmentNextLessonResponseDTO>(nextLesson.lesson);
        response.EnrollmentID = learningPath.EnrollmentID;
        response.CourseID = learningPath.CourseID;
        response.CourseVersionID = learningPath.CourseVersionID;
        response.ModuleID = nextLesson.module.ModuleID;
        response.ModuleNumber = nextLesson.module.ModuleNumber;

        return response;
    }

    public async Task DeleteMyEnrollmentAsync(Guid enrollmentId)
    {
        var enrollment = await GetMyEnrollmentEntityOrThrowAsync(enrollmentId);

        await _unitOfWork.Enrollments.DeleteAsync(enrollment);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PaginationResult<IEnumerable<CoursesEnrollmentResponse>>> GetCoursesOfUser(Guid clubId, UserEnrollmentSearchRequest request)
    {
        if (clubId == Guid.Empty)
            throw new ValidationException("ClubId không hợp lệ.");

        request ??= new UserEnrollmentSearchRequest();
        var currentUserId = _currentUser.UserId;
        EnrollStatus? enrollmentStatus = request.EnrollmentStatus switch
        {
            UserEnrollment.ACTIVE => EnrollStatus.ACTIVE,
            UserEnrollment.COMPLETED => EnrollStatus.COMPLETED,
            _ => null
        };

        var result = await _unitOfWork.Enrollments.GetCoursesOfUserAsync(
            userId: currentUserId,
            clubId: clubId,
            pageIndex: request.CurrentPage,
            pageSize: request.PageSize,
            level: request.Level,
            courseSearchName: request.CourseSearchName,
            enrollmentStatus: enrollmentStatus);

        var mapped = result.Data.Select(x => new CoursesEnrollmentResponse
        {
            EnrollmentId = x.EnrollmentId,
            CourseId = x.CourseId,
            CourseVersionId = x.CourseVersionId,
            CourseNameVN = x.CourseNameVN,
            CourseNameEN = x.CourseNameEN,
            ImageUrl = x.ImageUrl,
            Level = x.Level,
            EstimatedDuration = x.EstimatedDuration,
            Progress = x.Progress,
            EnrollStatus = x.EnrollStatus
        }).ToList();

        return new PaginationResult<IEnumerable<CoursesEnrollmentResponse>>(
            mapped,
            result.TotalRecords,
            result.PageIndex,
            result.PageSize);
    }

    private async Task<Enrollment> GetMyEnrollmentEntityOrThrowAsync(Guid enrollmentId)
    {
        var userId = _currentUser.UserId;

        var enrollment = await _unitOfWork.Enrollments.GetByConditionAsync(
            x => x.EnrollmentID == enrollmentId && x.UserID == userId);

        if (enrollment == null)
            throw new BaseException("Không tìm thấy enrollment.", "NOT_FOUND");

        return enrollment;
    }

    private async Task<List<Module>> GetModules(Guid courseVersionId)
    {
        var modulesResult = await _unitOfWork.Modules.GetAllAsync(
            filter: x => x.CourseVersionID == courseVersionId,
            orderBy: q => q.OrderBy(x => x.ModuleNumber),
            pageIndex: 1,
            pageSize: 10000);

        return modulesResult.Data.ToList();
    }

    private async Task<List<Lesson>> GetLessons(IEnumerable<Module> modules)
    {
        var moduleIds = modules.Select(x => x.ModuleID).Distinct().ToList();

        var lessonsResult = await _unitOfWork.Lessons.GetAllAsync(
            filter: x => moduleIds.Contains(x.ModuleID),
            orderBy: q => q.OrderBy(x => x.ModuleID).ThenBy(x => x.OrderIndex),
            pageIndex: 1,
            pageSize: 10000);

        return lessonsResult.Data.ToList();
    }

    private async Task<Dictionary<Guid, UserLesson>> GetUserLessons(Guid userId, IEnumerable<Lesson> lessons)
    {
        var lessonIds = lessons.Select(x => x.LessonID).Distinct().ToList();
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

    private EnrollmentLearningPathResponseDTO BuildEmptyResponse(Enrollment enrollment)
    {
        var response = _mapper.Map<EnrollmentLearningPathResponseDTO>(enrollment);
        response.Modules = [];
        return response;
    }

    private EnrollmentLearningPathResponseDTO BuildLearningPathResponse(
        Enrollment enrollment,
        IEnumerable<Module> modules,
        IEnumerable<Lesson> lessons,
        IReadOnlyDictionary<Guid, UserLesson> userLessons)
    {
        var lessonsByModule = lessons
            .GroupBy(x => x.ModuleID)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.OrderIndex).ToList());

        var responseModules = modules
            .Select(module =>
            {
                var moduleLessons = lessonsByModule.TryGetValue(module.ModuleID, out var list)
                    ? list
                    : [];

                var lessonDtos = moduleLessons
                    .Select(lesson =>
                    {
                        var dto = _mapper.Map<EnrollmentLearningPathLessonDTO>(lesson);
                        userLessons.TryGetValue(lesson.LessonID, out var userLesson);
                        dto.Status = userLesson?.Status;
                        dto.Progress = userLesson?.Progress;
                        dto.LastAccessDate = userLesson?.LastAccessDate;
                        return dto;
                    })
                    .ToList();

                var moduleDto = _mapper.Map<EnrollmentLearningPathModuleDTO>(module);
                moduleDto.Lessons = lessonDtos;
                return moduleDto;
            })
            .ToList();

        var response = _mapper.Map<EnrollmentLearningPathResponseDTO>(enrollment);
        response.Modules = responseModules;
        return response;
    }
}
