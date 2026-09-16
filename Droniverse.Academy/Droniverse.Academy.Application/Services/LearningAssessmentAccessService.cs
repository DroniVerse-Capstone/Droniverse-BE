using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public sealed class LearningAssessmentAccessService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILearningService _learningService;

    public LearningAssessmentAccessService(IUnitOfWork unitOfWork, ILearningService learningService)
    {
        _unitOfWork = unitOfWork;
        _learningService = learningService;
    }

    public async Task<(Quiz Quiz, Lesson Lesson)> GetAccessibleQuizAsync(Guid enrollmentId, Guid quizId)
    {
        var quiz = await _unitOfWork.Quizs.GetByIdAsync(quizId)
            ?? throw new NotFoundException("Không tìm thấy quiz.");

        var lesson = await _unitOfWork.Lessons.GetByConditionAsync(x => x.Type == LessonType.QUIZ && x.ReferenceID == quizId)
            ?? throw new NotFoundException("Không tìm thấy lesson của quiz.");

        await _learningService.ValidateLessonAccessAsync(enrollmentId, lesson.LessonID);
        return (quiz, lesson);
    }

    public async Task<(Lab Lab, Lesson Lesson)> GetAccessibleLabAsync(Guid enrollmentId, Guid labId)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(labId)
            ?? throw new NotFoundException("Không tìm thấy lab.");

        var lesson = await _unitOfWork.Lessons.GetByConditionAsync(x => x.Type == LessonType.LAB && x.ReferenceID == labId)
            ?? throw new NotFoundException("Không tìm thấy lesson của lab.");

        await _learningService.ValidateLessonAccessAsync(enrollmentId, lesson.LessonID);
        return (lab, lesson);
    }

    public async Task<(Assignment Assignment, Lesson Lesson)> GetAccessibleAssignmentAsync(Guid enrollmentId, Guid assignmentId)
    {
        var assignment = await _unitOfWork.Assignments.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Không tìm thấy assignment.");

        var lesson = await _unitOfWork.Lessons.GetByConditionAsync(
                         x => x.Type == LessonType.ASSIGNMENT && x.ReferenceID == assignmentId)
                     ?? throw new NotFoundException("Không tìm thấy lesson của assignment.");

        await _learningService.ValidateLessonAccessAsync(enrollmentId, lesson.LessonID);
        return (assignment, lesson);
    }
}
