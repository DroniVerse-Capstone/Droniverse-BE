using Droniverse.Academy.Domain.Entities;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Helpers;

public static class LearningValidator
{
    public static Enrollment EnsureEnrollmentOwnedByUser(Enrollment? enrollment)
    {
        return enrollment ?? throw new NotFoundException("Không tìm thấy enrollment.");
    }

    public static Lesson EnsureLessonExists(Lesson? lesson)
    {
        return lesson ?? throw new NotFoundException("Không tìm thấy lesson.");
    }

    public static void EnsureLessonInCourseVersion(Lesson lesson, IReadOnlyCollection<Guid> moduleIds)
    {
        if (!moduleIds.Contains(lesson.ModuleID))
            throw new ForbiddenException("Lesson không thuộc enrollment hiện tại.");
    }

    public static void EnsureLessonAccessible(bool isLocked)
    {
        if (isLocked)
            throw new ForbiddenException("Lesson chưa được mở. Vui lòng hoàn thành bài học trước đó.");
    }

    public static void EnsureSubmitAnswers(IReadOnlyCollection<Guid> questionIds, int answerCount)
    {
        if (answerCount == 0)
            throw new BadRequestException("Danh sách câu trả lời không được để trống.");

        if (questionIds.Count == 0)
            throw new NotFoundException("Quiz chưa có câu hỏi.");
    }
}
