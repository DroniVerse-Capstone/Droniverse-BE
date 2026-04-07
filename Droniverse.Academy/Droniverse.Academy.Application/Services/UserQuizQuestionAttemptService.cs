using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class UserQuizQuestionAttemptService : IUserQuizQuestionAttemptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public UserQuizQuestionAttemptService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<UserQuizQuestionAttemptResponseDTO> CreateUserQuizQuestionAttemptAsync(CreateUserQuizQuestionAttemptRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var selectedAnswer = NormalizeAnswer(request.SelectedAnswer);

        var attempt = await _unitOfWork.QuizAttempts.GetByConditionAsync(x => x.AttemptID == request.AttemptID);
        if (attempt == null)
            throw new BaseException("Không tìm thấy quiz attempt.", "NOT_FOUND");

        if (attempt.UserID != _currentUser.UserId)
            throw new ValidationException("Bạn không có quyền thao tác quiz attempt này.");

        var question = await _unitOfWork.QuizQuestions.GetByIdAsync(request.QuestionID);
        if (question == null)
            throw new BaseException("Không tìm thấy quiz question.", "NOT_FOUND");

        if (question.QuizID != attempt.QuizID)
            throw new ValidationException("Question không thuộc quiz của attempt.");

        var existing = await _unitOfWork.QuizQuestionAttempts.GetByConditionAsync(
            x => x.AttemptID == request.AttemptID && x.QuestionID == request.QuestionID);

        if (existing != null)
            throw new ValidationException("Câu hỏi này đã được trả lời trong attempt.");

        var answer = _mapper.Map<QuizQuestionAttempt>(request);
        answer.AttemptAnswerID = Guid.NewGuid();
        answer.SelectedAnswer = selectedAnswer;
        answer.IsCorrect = string.Equals(selectedAnswer, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
        answer.Score = answer.IsCorrect ? question.Score : 0f;

        await _unitOfWork.QuizQuestionAttempts.AddAsync(answer);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserQuizQuestionAttemptResponseDTO>(answer);
    }

    public async Task<PaginationResult<IEnumerable<UserQuizQuestionAttemptResponseDTO>>> GetMyQuizQuestionAttemptsAsync(int pageIndex = 1, int pageSize = 10, bool? isCorrect = null)
    {
        var userId = _currentUser.UserId;

        var myAttempts = await _unitOfWork.QuizAttempts.GetAllAsync(
            x => x.UserID == userId,
            pageIndex: 1,
            pageSize: int.MaxValue);

        var attemptIds = myAttempts.Data.Select(x => x.AttemptID).ToList();
        if (attemptIds.Count == 0)
            return new PaginationResult<IEnumerable<UserQuizQuestionAttemptResponseDTO>>(new List<UserQuizQuestionAttemptResponseDTO>(), 0, pageIndex, pageSize);

        var result = await _unitOfWork.QuizQuestionAttempts.GetAllAsync(
            isCorrect.HasValue
                ? x => attemptIds.Contains(x.AttemptID) && x.IsCorrect == isCorrect.Value
                : x => attemptIds.Contains(x.AttemptID),
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.AttemptAnswerID));

        var mapped = result.Data.Select(x => _mapper.Map<UserQuizQuestionAttemptResponseDTO>(x)).ToList();
        return new PaginationResult<IEnumerable<UserQuizQuestionAttemptResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<UserQuizQuestionAttemptResponseDTO> GetMyQuizQuestionAttemptByIdAsync(Guid attemptAnswerId)
    {
        var answer = await _unitOfWork.QuizQuestionAttempts.GetByConditionAsync(
            x => x.AttemptAnswerID == attemptAnswerId,
            includeProperties: "QuizAttempt");

        if (answer == null)
            throw new BaseException("Không tìm thấy quiz question attempt.", "NOT_FOUND");

        if (answer.QuizAttempt.UserID != _currentUser.UserId)
            throw new ValidationException("Bạn không có quyền truy cập quiz question attempt này.");

        return _mapper.Map<UserQuizQuestionAttemptResponseDTO>(answer);
    }

    public async Task<UserQuizQuestionAttemptResponseDTO> UpdateMyQuizQuestionAttemptAsync(Guid attemptAnswerId, UpdateUserQuizQuestionAttemptRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var selectedAnswer = NormalizeAnswer(request.SelectedAnswer);

        var answer = await _unitOfWork.QuizQuestionAttempts.GetByConditionAsync(
            x => x.AttemptAnswerID == attemptAnswerId,
            includeProperties: "QuizAttempt,QuizQuestion");

        if (answer == null)
            throw new BaseException("Không tìm thấy quiz question attempt.", "NOT_FOUND");

        if (answer.QuizAttempt.UserID != _currentUser.UserId)
            throw new ValidationException("Bạn không có quyền cập nhật quiz question attempt này.");

        answer.SelectedAnswer = selectedAnswer;
        answer.IsCorrect = string.Equals(selectedAnswer, answer.QuizQuestion.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
        answer.Score = answer.IsCorrect ? answer.QuizQuestion.Score : 0f;

        await _unitOfWork.QuizQuestionAttempts.UpdateAsync(answer);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserQuizQuestionAttemptResponseDTO>(answer);
    }

    public async Task DeleteMyQuizQuestionAttemptAsync(Guid attemptAnswerId)
    {
        var answer = await _unitOfWork.QuizQuestionAttempts.GetByConditionAsync(
            x => x.AttemptAnswerID == attemptAnswerId,
            includeProperties: "QuizAttempt");

        if (answer == null)
            throw new BaseException("Không tìm thấy quiz question attempt.", "NOT_FOUND");

        if (answer.QuizAttempt.UserID != _currentUser.UserId)
            throw new ValidationException("Bạn không có quyền xóa quiz question attempt này.");

        await _unitOfWork.QuizQuestionAttempts.DeleteAsync(answer);
        await _unitOfWork.SaveChangesAsync();
    }

    private static string NormalizeAnswer(string selectedAnswer)
    {
        if (string.IsNullOrWhiteSpace(selectedAnswer))
            throw new ValidationException("SelectedAnswer không được để trống.");

        var normalized = selectedAnswer.Trim().ToUpperInvariant();
        if (normalized is not ("A" or "B" or "C" or "D"))
            throw new ValidationException("SelectedAnswer phải là A, B, C hoặc D.");

        return normalized;
    }
}
