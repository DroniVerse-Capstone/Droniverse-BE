using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services;

public class QuizQuestionService : IQuizQuestionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public QuizQuestionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<QuizQuestionClientViewDTO> CreateQuizQuestionAsync(Guid quizId, CreateQuizQuestionRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateQuizQuestionData(
            request.ContentVN,
            request.ContentEN,
            request.AnswerA,
            request.AnswerB,
            request.AnswerC,
            request.AnswerD,
            request.AnswerA_EN,
            request.AnswerB_EN,
            request.AnswerC_EN,
            request.AnswerD_EN,
            request.CorrectAnswer,
            request.Score);

        var quiz = await _unitOfWork.Quizs.GetByIdAsync(quizId);
        if (quiz == null)
            throw new BaseException("Không tìm thấy bài kiểm tra.", "NOT_FOUND");

        var question = _mapper.Map<QuizQuestion>(request);
        question.QuestionID = Guid.NewGuid();
        question.QuizID = quizId;
        question.CorrectAnswer = request.CorrectAnswer.Trim().ToUpperInvariant();

        await _unitOfWork.QuizQuestions.AddAsync(question);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<QuizQuestionClientViewDTO>(question);
    }

    public async Task<IEnumerable<QuizQuestionClientViewDTO>> GetQuizQuestionsByQuizIdAsync(Guid quizId)
    {
        var quiz = await _unitOfWork.Quizs.GetByIdAsync(quizId);
        if (quiz == null)
            throw new BaseException("Không tìm thấy bài kiểm tra.", "NOT_FOUND");

        var questions = await _unitOfWork.QuizQuestions.GetAllAsync(
            filter: q => q.QuizID == quizId,
            orderBy: q => q.OrderBy(x => x.QuestionID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return _mapper.Map<IEnumerable<QuizQuestionClientViewDTO>>(questions.Data);
    }

    public async Task<QuizQuestionClientViewDTO> GetQuizQuestionByIdAsync(Guid quizId, Guid questionId)
    {
        var question = await GetQuestionInQuizAsync(quizId, questionId);
        if (question is null)
            throw new BaseException("Không tìm thấy câu hỏi quiz.", "NOT_FOUND");

        return _mapper.Map<QuizQuestionClientViewDTO>(question);
    }

    public async Task<QuizQuestionClientViewDTO> UpdateQuizQuestionAsync(Guid quizId, Guid questionId, UpdateQuizQuestionRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateQuizQuestionData(
            request.ContentVN,
            request.ContentEN,
            request.AnswerA,
            request.AnswerB,
            request.AnswerC,
            request.AnswerD,
            request.AnswerA_EN,
            request.AnswerB_EN,
            request.AnswerC_EN,
            request.AnswerD_EN,
            request.CorrectAnswer,
            request.Score);

        var question = await GetQuestionInQuizAsync(quizId, questionId);
        if (question is null)
            throw new BaseException("Không tìm thấy câu hỏi quiz.", "NOT_FOUND");

        _mapper.Map(request, question);
        question.CorrectAnswer = request.CorrectAnswer.Trim().ToUpperInvariant();

        await _unitOfWork.QuizQuestions.UpdateAsync(question);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<QuizQuestionClientViewDTO>(question);
    }

    public async Task DeleteQuizQuestionAsync(Guid quizId, Guid questionId)
    {
        var question = await GetQuestionInQuizAsync(quizId, questionId);
        if (question is null)
            throw new BaseException("Không tìm thấy câu hỏi quiz.", "NOT_FOUND");

        await _unitOfWork.QuizQuestions.DeleteAsync(question);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<QuizQuestion?> GetQuestionInQuizAsync(Guid quizId, Guid questionId)
    {
        return await _unitOfWork.QuizQuestions.GetByConditionAsync(q => q.QuestionID == questionId && q.QuizID == quizId);
    }

    private static void ValidateQuizQuestionData(
        string contentVN,
        string contentEN,
        string answerA,
        string answerB,
        string answerC,
        string answerD,
        string answerA_EN,
        string answerB_EN,
        string answerC_EN,
        string answerD_EN,
        string correctAnswer,
        float score)
    {
        if (string.IsNullOrWhiteSpace(contentVN))
            throw new ValidationException("Nội dung tiếng Việt là bắt buộc.");

        if (string.IsNullOrWhiteSpace(contentEN))
            throw new ValidationException("Nội dung tiếng Anh là bắt buộc.");

        if (string.IsNullOrWhiteSpace(answerA) ||
            string.IsNullOrWhiteSpace(answerB) ||
            string.IsNullOrWhiteSpace(answerC) ||
            string.IsNullOrWhiteSpace(answerD))
            throw new ValidationException("Các đáp án A, B, C, D là bắt buộc.");

        if (string.IsNullOrWhiteSpace(answerA_EN) ||
            string.IsNullOrWhiteSpace(answerB_EN) ||
            string.IsNullOrWhiteSpace(answerC_EN) ||
            string.IsNullOrWhiteSpace(answerD_EN))
            throw new ValidationException("Các đáp án tiếng Anh A, B, C, D là bắt buộc.");

        if (score <= 0)
            throw new ValidationException("Điểm câu hỏi phải lớn hơn 0.");

        if (string.IsNullOrWhiteSpace(correctAnswer))
            throw new ValidationException("Đáp án đúng là bắt buộc.");

        var normalizedAnswer = correctAnswer.Trim().ToUpperInvariant();
        if (normalizedAnswer is not ("A" or "B" or "C" or "D"))
            throw new ValidationException("Đáp án đúng chỉ được là A, B, C hoặc D.");
    }
}
