using Droniverse.Academy.Application.Helpers;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public sealed class LearningContextLoader
{
    private readonly IUnitOfWork _unitOfWork;

    public LearningContextLoader(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Enrollment> GetEnrollmentAsync(Guid enrollmentId, Guid userId)
    {
        var enrollment = await _unitOfWork.Enrollments.GetByConditionAsync(
            x => x.EnrollmentID == enrollmentId && x.UserID == userId);

        return LearningValidator.EnsureEnrollmentOwnedByUser(enrollment);
    }

    public async Task<CourseVersion> GetCourseVersionAsync(Guid courseVersionId)
    {
        var courseVersion = await _unitOfWork.CourseVersions.GetByIdAsync(courseVersionId);
        if (courseVersion == null)
            throw new NotFoundException("Không tìm thấy phiên bản khóa học.");

        return courseVersion;
    }

    public async Task<List<Module>> GetModulesByCourseVersionAsync(Guid courseVersionId)
    {
        var modulesResult = await _unitOfWork.Modules.GetAllAsync(
            filter: x => x.CourseVersionID == courseVersionId,
            orderBy: q => q.OrderBy(x => x.ModuleNumber),
            pageIndex: 1,
            pageSize: 10000);

        return modulesResult.Data.ToList();
    }

    public async Task<List<Lesson>> GetLessonsByModuleIdsAsync(IReadOnlyCollection<Guid> moduleIds)
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

    public async Task<Dictionary<Guid, UserLesson>> GetUserLessonsLookupAsync(Guid userId, IReadOnlyCollection<Guid> lessonIds)
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

    public async Task<Dictionary<Guid, UserModule>> GetUserModulesLookupAsync(Guid userId, IReadOnlyCollection<Guid> moduleIds)
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
}
