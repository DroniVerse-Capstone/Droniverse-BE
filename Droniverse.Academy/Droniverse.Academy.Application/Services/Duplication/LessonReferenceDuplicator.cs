using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.Services.Duplication;

public class LessonReferenceDuplicator : ILessonReferenceDuplicator
{
    private readonly ITheoryDuplicator _theoryDuplicator;
    private readonly IQuizDuplicator _quizDuplicator;

    public LessonReferenceDuplicator(
        ITheoryDuplicator theoryDuplicator,
        IQuizDuplicator quizDuplicator)
    {
        _theoryDuplicator = theoryDuplicator;
        _quizDuplicator = quizDuplicator;
    }

    public async Task<Guid> DuplicateAsync(Lesson sourceLesson, CourseVersionDuplicationContext context)
    {
        if (sourceLesson.ReferenceID == Guid.Empty)
            return Guid.Empty;

        // Sao chép nội dung của bài học dựa trên LessonType bằng cách gọi các duplicator tương ứng. Trả về ReferenceID mới sau khi sao chép.
        return sourceLesson.Type switch
        {
            LessonType.THEORY => await _theoryDuplicator.DuplicateAsync(sourceLesson.ReferenceID, context),
            LessonType.QUIZ => await _quizDuplicator.DuplicateAsync(sourceLesson.ReferenceID, context),
            LessonType.LAB => sourceLesson.ReferenceID,
            LessonType.PHYSIC => sourceLesson.ReferenceID,
            LessonType.LAB_PHYSIC => sourceLesson.ReferenceID,
            LessonType.VR => sourceLesson.ReferenceID,
            LessonType.ASSIGNMENT => sourceLesson.ReferenceID,
            _ => sourceLesson.ReferenceID
        };
    }
}
