using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.Services.Duplication;

public class LessonReferenceDuplicator : ILessonReferenceDuplicator
{
    private readonly ITheoryDuplicator _theoryDuplicator;
    private readonly IQuizDuplicator _quizDuplicator;
    private readonly ILabDuplicator _labDuplicator;

    public LessonReferenceDuplicator(
        ITheoryDuplicator theoryDuplicator,
        IQuizDuplicator quizDuplicator,
        ILabDuplicator labDuplicator)
    {
        _theoryDuplicator = theoryDuplicator;
        _quizDuplicator = quizDuplicator;
        _labDuplicator = labDuplicator;
    }

    public async Task<Guid> DuplicateAsync(Lesson sourceLesson, CourseVersionDuplicationContext context)
    {
        if (sourceLesson.ReferenceID == Guid.Empty)
            return Guid.Empty;

        return sourceLesson.Type switch
        {
            LessonType.THEORY => await _theoryDuplicator.DuplicateAsync(sourceLesson.ReferenceID, context),
            LessonType.QUIZ => await _quizDuplicator.DuplicateAsync(sourceLesson.ReferenceID, context),
            LessonType.LAB => await _labDuplicator.DuplicateAsync(sourceLesson.ReferenceID, context),
            _ => sourceLesson.ReferenceID
        };
    }
}
