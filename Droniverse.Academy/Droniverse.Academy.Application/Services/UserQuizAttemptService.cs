using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;

namespace Droniverse.Academy.Application.Services;

public class UserQuizAttemptService : IUserQuizAttemptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public UserQuizAttemptService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<UserQuizAttemptResponseDTO> CreateUserQuizAttemptAsync(CreateUserQuizAttemptRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (await _unitOfWork.Quizs.GetByIdAsync(request.QuizID) == null)
            throw new BaseException("Không tìm thấy quiz.", "NOT_FOUND");

        var attempt = _mapper.Map<QuizAttempt>(request);
        attempt.AttemptID = Guid.NewGuid();
        attempt.UserID = _currentUser.UserId;
        attempt.StartTime = _clock.Now;
        attempt.SubmitTime = null;
        attempt.Score = null;
        attempt.IsPassed = false;

        await _unitOfWork.QuizAttempts.AddAsync(attempt);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserQuizAttemptResponseDTO>(attempt);
    }

    public async Task<PaginationResult<IEnumerable<UserQuizAttemptResponseDTO>>> GetMyQuizAttemptsAsync(int pageIndex = 1, int pageSize = 10, bool? isPassed = null)
    {
        var userId = _currentUser.UserId;

        var result = await _unitOfWork.QuizAttempts.GetAllAsync(
            isPassed.HasValue
                ? x => x.UserID == userId && x.IsPassed == isPassed.Value
                : x => x.UserID == userId,
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.StartTime));

        var mapped = result.Data.Select(x => _mapper.Map<UserQuizAttemptResponseDTO>(x)).ToList();
        return new PaginationResult<IEnumerable<UserQuizAttemptResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<UserQuizAttemptResponseDTO> GetMyQuizAttemptByIdAsync(Guid attemptId)
    {
        var userId = _currentUser.UserId;

        var attempt = await _unitOfWork.QuizAttempts.GetByConditionAsync(
            x => x.AttemptID == attemptId && x.UserID == userId);

        if (attempt == null)
            throw new BaseException("Không tìm thấy quiz attempt.", "NOT_FOUND");

        return _mapper.Map<UserQuizAttemptResponseDTO>(attempt);
    }

    public async Task<UserQuizAttemptResponseDTO> UpdateMyQuizAttemptAsync(Guid attemptId, UpdateUserQuizAttemptRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var userId = _currentUser.UserId;

        var attempt = await _unitOfWork.QuizAttempts.GetByConditionAsync(
            x => x.AttemptID == attemptId && x.UserID == userId);

        if (attempt == null)
            throw new BaseException("Không tìm thấy quiz attempt.", "NOT_FOUND");

        var quiz = await _unitOfWork.Quizs.GetByIdAsync(attempt.QuizID);
        if (quiz == null)
            throw new BaseException("Không tìm thấy quiz.", "NOT_FOUND");

        attempt.SubmitTime = request.SubmitTime ?? _clock.Now;

        var answers = await _unitOfWork.QuizQuestionAttempts.GetAllAsync(
            x => x.AttemptID == attemptId,
            pageIndex: 1,
            pageSize: int.MaxValue);

        var totalScore = answers.Data.Sum(x => x.Score ?? 0f);
        attempt.Score = totalScore;
        attempt.IsPassed = totalScore >= quiz.PassScore;

        await _unitOfWork.QuizAttempts.UpdateAsync(attempt);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserQuizAttemptResponseDTO>(attempt);
    }

    public async Task DeleteMyQuizAttemptAsync(Guid attemptId)
    {
        var userId = _currentUser.UserId;

        var attempt = await _unitOfWork.QuizAttempts.GetByConditionAsync(
            x => x.AttemptID == attemptId && x.UserID == userId);

        if (attempt == null)
            throw new BaseException("Không tìm thấy quiz attempt.", "NOT_FOUND");

        await _unitOfWork.QuizAttempts.DeleteAsync(attempt);
        await _unitOfWork.SaveChangesAsync();
    }
}
