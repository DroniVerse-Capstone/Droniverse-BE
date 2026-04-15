using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.Helpers;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class QuizLearningService : IQuizLearningService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly ILearningService _learningService;
    private readonly LearningAssessmentAccessService _assessmentAccessService;
    private readonly IMapper _mapper;

    public QuizLearningService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IClock clock,
        ILearningService learningService,
        LearningAssessmentAccessService assessmentAccessService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
        _learningService = learningService;
        _assessmentAccessService = assessmentAccessService;
        _mapper = mapper;
    }

    public async Task<QuizLearningStateDTO> GetQuizAttemptOrQuizAsync(Guid enrollmentId, Guid quizId)
    {
        var (quiz, _) = await _assessmentAccessService.GetAccessibleQuizAsync(enrollmentId, quizId);

        var latestAttemptResult = await _unitOfWork.QuizAttempts.GetAllAsync(
            filter: x => x.QuizID == quizId && x.UserID == _currentUser.UserId,
            orderBy: q => q
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.SubmitTime)
                .ThenByDescending(x => x.StartTime),
            pageIndex: 1,
            pageSize: 1,
            includeProperties: "Quiz");

        var latestAttempt = latestAttemptResult.Data.FirstOrDefault();
        return new QuizLearningStateDTO
        {
            Quiz = _mapper.Map<QuizClientViewDTO>(quiz),
            Attempt = latestAttempt == null
                ? null
                : _mapper.Map<QuizAttemptDTO>(latestAttempt)
        };
    }

    public async Task<QuizAttemptReviewDTO> GetLatestQuizAttemptReviewAsync(Guid enrollmentId, Guid quizId)
    {
        var (quiz, _) = await _assessmentAccessService.GetAccessibleQuizAsync(enrollmentId, quizId);

        var latestAttemptResult = await _unitOfWork.QuizAttempts.GetAllAsync(
            filter: x => x.QuizID == quizId && x.UserID == _currentUser.UserId,
            orderBy: q => q.OrderByDescending(x => x.StartTime),
            pageIndex: 1,
            pageSize: 1);

        var latestAttempt = latestAttemptResult.Data.FirstOrDefault()
            ?? throw new NotFoundException("Chưa có bài làm quiz để xem lại.");

        var questionAttemptsResult = await _unitOfWork.QuizQuestionAttempts.GetAllAsync(
            filter: x => x.AttemptID == latestAttempt.AttemptID,
            orderBy: q => q.OrderBy(x => x.AttemptAnswerID),
            pageIndex: 1,
            pageSize: 10000,
            includeProperties: "QuizQuestion");

        return new QuizAttemptReviewDTO
        {
            Quiz = _mapper.Map<QuizClientViewDTO>(quiz),
            Attempt = _mapper.Map<QuizAttemptDTO>(latestAttempt),
            Questions = _mapper.Map<List<QuizQuestionAttemptReviewDTO>>(questionAttemptsResult.Data)
        };
    }

    public async Task<IEnumerable<QuizQuestionLearningDTO>> GetQuizQuestionsForLearningAsync(Guid enrollmentId, Guid quizId)
    {
        var (quiz, _) = await _assessmentAccessService.GetAccessibleQuizAsync(enrollmentId, quizId);

        var questionsResult = await _unitOfWork.QuizQuestions.GetAllAsync(
            filter: x => x.QuizID == quizId,
            pageIndex: 1,
            pageSize: 10000);

        var shuffledQuestions = questionsResult.Data
            .Select(x => MapLearningQuestion(x, quiz))
            .ToList();

        ShuffleInPlace(shuffledQuestions);
        return shuffledQuestions;
    }

    public async Task<SubmitQuizResultDTO> SubmitQuizAsync(Guid enrollmentId, Guid quizId, SubmitQuizRequestDTO request)
    {
        ValidateRequest(request);

        var (quiz, lesson) = await _assessmentAccessService.GetAccessibleQuizAsync(enrollmentId, quizId);

        var questionMap = await GetQuizQuestionMapAsync(quizId, request.Answers.Count);
        var calculatedResult = CalculateQuizResult(request, questionMap, quizId);

        ApplyPassResult(calculatedResult.Attempt, quiz.PassScore);
        await SaveQuizAttemptAsync(calculatedResult.Attempt, calculatedResult.Answers);

        var completion = await CompleteLessonIfPassedAsync(
            enrollmentId,
            lesson.LessonID,
            calculatedResult.Attempt.IsPassed);

        var bestScore = await GetBestScoreAsync(quizId, calculatedResult.Attempt.Score ?? 0f);

        return BuildSubmitResult(calculatedResult.Attempt, bestScore, completion);
    }

    private static void ValidateRequest(SubmitQuizRequestDTO request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }

    private async Task<Dictionary<Guid, QuizQuestion>> GetQuizQuestionMapAsync(Guid quizId, int answerCount)
    {
        var questionsResult = await _unitOfWork.QuizQuestions.GetAllAsync(
            filter: x => x.QuizID == quizId,
            pageIndex: 1,
            pageSize: 10000);

        var questionMap = questionsResult.Data.ToDictionary(x => x.QuestionID, x => x);
        LearningValidator.EnsureSubmitAnswers(questionMap.Keys.ToList(), answerCount);
        return questionMap;
    }

    private QuizCalculationResult CalculateQuizResult(
        SubmitQuizRequestDTO request,
        IReadOnlyDictionary<Guid, QuizQuestion> questionMap,
        Guid quizId)
    {
        var attemptId = Guid.NewGuid();
        var submitTime = _clock.Now;

        var normalizedAnswers = request.Answers
            .GroupBy(x => x.QuestionID)
            .Select(x => x.First())
            .ToList();

        var answerAttempts = new List<QuizQuestionAttempt>();
        float totalScore = 0;

        foreach (var answer in normalizedAnswers)
        {
            if (!questionMap.TryGetValue(answer.QuestionID, out var question))
                continue;

            var selectedAnswer = NormalizeAnswer(answer);
            var isCorrect = string.Equals(selectedAnswer, question.CorrectAnswer, StringComparison.OrdinalIgnoreCase);
            var answerScore = isCorrect ? question.Score : 0f;

            totalScore += answerScore;
            answerAttempts.Add(BuildQuizQuestionAttempt(attemptId, question.QuestionID, selectedAnswer, isCorrect, answerScore));
        }

        var attempt = new QuizAttempt
        {
            AttemptID = attemptId,
            QuizID = quizId,
            UserID = _currentUser.UserId,
            StartTime = submitTime,
            SubmitTime = submitTime,
            Score = totalScore,
            IsPassed = false
        };

        return new QuizCalculationResult(attempt, answerAttempts);
    }

    private static QuizQuestionAttempt BuildQuizQuestionAttempt(
        Guid attemptId,
        Guid questionId,
        string selectedAnswer,
        bool isCorrect,
        float answerScore)
    {
        return new QuizQuestionAttempt
        {
            AttemptAnswerID = Guid.NewGuid(),
            AttemptID = attemptId,
            QuestionID = questionId,
            SelectedAnswer = selectedAnswer,
            IsCorrect = isCorrect,
            Score = answerScore
        };
    }

    private static void ApplyPassResult(QuizAttempt attempt, float passScore)
    {
        attempt.IsPassed = (attempt.Score ?? 0f) >= passScore;
    }

    private async Task SaveQuizAttemptAsync(QuizAttempt attempt, IReadOnlyCollection<QuizQuestionAttempt> answers)
    {
        await _unitOfWork.QuizAttempts.AddAsync(attempt);
        await _unitOfWork.QuizQuestionAttempts.AddRangeAsync(answers);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<CompleteLessonResultDTO?> CompleteLessonIfPassedAsync(Guid enrollmentId, Guid lessonId, bool isPassed)
    {
        if (!isPassed)
            return null;

        return await _learningService.CompleteLessonByAssessmentAsync(enrollmentId, lessonId);
    }

    private async Task<float> GetBestScoreAsync(Guid quizId, float fallbackScore)
    {
        var attemptsResult = await _unitOfWork.QuizAttempts.GetAllAsync(
            filter: x => x.UserID == _currentUser.UserId && x.QuizID == quizId,
            orderBy: q => q.OrderByDescending(x => x.Score),
            pageIndex: 1,
            pageSize: 1);

        return attemptsResult.Data.FirstOrDefault()?.Score ?? fallbackScore;
    }

    private static SubmitQuizResultDTO BuildSubmitResult(
        QuizAttempt attempt,
        float bestScore,
        CompleteLessonResultDTO? completion)
    {
        return new SubmitQuizResultDTO
        {
            AttemptID = attempt.AttemptID,
            Score = attempt.Score ?? 0f,
            IsPassed = attempt.IsPassed,
            BestScore = bestScore,
            Completion = completion
        };
    }

    private static string NormalizeAnswer(SubmitQuizAnswerRequestDTO answer)
    {
        var selectedValue = answer.SelectedOptionKey;

        if (string.IsNullOrWhiteSpace(selectedValue))
            throw new BadRequestException("SelectedOptionKey không được để trống.");

        if (TryNormalizeOptionKey(selectedValue, out var normalized))
            return normalized;

        throw new BadRequestException("SelectedOptionKey phải là A, B, C hoặc D.");
    }

    private static bool TryNormalizeOptionKey(string? value, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var key = value.Trim().ToUpperInvariant();
        if (key is not ("A" or "B" or "C" or "D"))
            return false;

        normalized = key;
        return true;
    }

    private static QuizQuestionLearningDTO MapLearningQuestion(QuizQuestion question, Quiz quiz)
    {
        var options = new List<QuizQuestionOptionLearningDTO>
        {
            new() { OptionKey = "A", ContentVN = question.AnswerA, ContentEN = question.AnswerA_EN },
            new() { OptionKey = "B", ContentVN = question.AnswerB, ContentEN = question.AnswerB_EN },
            new() { OptionKey = "C", ContentVN = question.AnswerC, ContentEN = question.AnswerC_EN },
            new() { OptionKey = "D", ContentVN = question.AnswerD, ContentEN = question.AnswerD_EN }
        };

        ShuffleInPlace(options);

        return new QuizQuestionLearningDTO
        {
            TitleVN = quiz.TitleVN,
            TitleEN = quiz.TitleEN,
            TimeLimit = quiz.TimeLimit,
            QuestionID = question.QuestionID,
            ContentVN = question.ContentVN,
            ContentEN = question.ContentEN,
            Options = options
        };
    }

    private static void ShuffleInPlace<T>(IList<T> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }

    private sealed record QuizCalculationResult(QuizAttempt Attempt, IReadOnlyCollection<QuizQuestionAttempt> Answers);
}
