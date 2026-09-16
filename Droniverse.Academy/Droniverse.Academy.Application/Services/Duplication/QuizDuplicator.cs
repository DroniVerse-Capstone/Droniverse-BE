using AutoMapper;
using Droniverse.Academy.Application.IService.Duplication;
using Droniverse.Academy.Application.Services.Duplication.Models;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services.Duplication;

public class QuizDuplicator : IQuizDuplicator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public QuizDuplicator(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Guid> DuplicateAsync(Guid sourceQuizId, CourseVersionDuplicationContext context)
    {
        if (context.QuizIdMap.TryGetValue(sourceQuizId, out var duplicatedQuizId))
            return duplicatedQuizId;

        var sourceQuiz = await _unitOfWork.Quizs.GetByIdAsync(sourceQuizId)
            ?? throw new BaseException("Không tìm thấy bài quiz tham chiếu để nhân bản.", "NOT_FOUND");

        var duplicatedQuiz = _mapper.Map<Quiz>(sourceQuiz);
        duplicatedQuiz.QuizID = Guid.NewGuid();
        duplicatedQuiz.SetAuditOnCreate(context.CurrentUserId, context.Now);

        await _unitOfWork.Quizs.AddAsync(duplicatedQuiz);
        await DuplicateQuestionsAsync(sourceQuizId, duplicatedQuiz.QuizID);

        context.QuizIdMap[sourceQuizId] = duplicatedQuiz.QuizID;
        return duplicatedQuiz.QuizID;
    }

    private async Task DuplicateQuestionsAsync(Guid sourceQuizId, Guid duplicatedQuizId)
    {
        var sourceQuestions = await _unitOfWork.QuizQuestions.GetAllAsync(
            filter: q => q.QuizID == sourceQuizId,
            orderBy: q => q.OrderBy(x => x.QuestionID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        foreach (var sourceQuestion in sourceQuestions.Data)
        {
            var duplicatedQuestion = _mapper.Map<QuizQuestion>(sourceQuestion);
            duplicatedQuestion.QuestionID = Guid.NewGuid();
            duplicatedQuestion.QuizID = duplicatedQuizId;

            await _unitOfWork.QuizQuestions.AddAsync(duplicatedQuestion);
        }
    }
}
