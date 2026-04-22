using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Helpers;

public static class CourseVersionDraftGuard
{
    public static async Task EnsureDraftByCourseVersionAsync(
        IUnitOfWork unitOfWork,
        Guid courseId,
        Guid versionId,
        string notDraftMessage)
    {
        var courseVersion = await unitOfWork.CourseVersions.GetByConditionAsync(
            v => v.CourseVersionID == versionId && v.CourseID == courseId)
            ?? throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (courseVersion.Status != CourseVersionStatus.DRAFT)
            throw new ValidationException(notDraftMessage);
    }

    public static async Task EnsureDraftByModuleIdAsync(
        IUnitOfWork unitOfWork,
        Guid moduleId,
        string notDraftMessage)
    {
        var module = await unitOfWork.Modules.GetByIdAsync(moduleId)
            ?? throw new BaseException("Không tìm thấy mô-đun.", "NOT_FOUND");

        var courseVersion = await unitOfWork.CourseVersions.GetByIdAsync(module.CourseVersionID)
            ?? throw new BaseException("Không tìm thấy phiên bản khóa học.", "NOT_FOUND");

        if (courseVersion.Status != CourseVersionStatus.DRAFT)
            throw new ValidationException(notDraftMessage);
    }

    public static async Task EnsureDraftByReferenceAsync(
        IUnitOfWork unitOfWork,
        Guid referenceId,
        LessonType lessonType,
        string lessonTypeDisplayName,
        string notDraftMessage)
    {
        var lesson = await unitOfWork.Lessons.GetByConditionAsync(
            l => l.Type == lessonType && l.ReferenceID == referenceId)
            ?? throw new ValidationException($"Không tìm thấy lesson {lessonTypeDisplayName} tham chiếu.");

        await EnsureDraftByModuleIdAsync(unitOfWork, lesson.ModuleID, notDraftMessage);
    }
}
