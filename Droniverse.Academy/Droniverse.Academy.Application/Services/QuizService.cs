using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;

namespace Droniverse.Academy.Application.Services;

public class QuizService : IQuizService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public QuizService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<QuizClientViewDTO> CreateQuizAsync(CreateQuizRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateQuizData(request.TimeLimit, request.TotalScore, request.PassScore);

        var lesson = await _unitOfWork.Lessons.GetByIdAsync(request.LessonID);
        if (lesson == null)
            throw new BaseException("Lesson not found.", "NOT_FOUND");

        if (lesson.Type != LessonType.QUIZ)
            throw new ValidationException("Lesson type must be QUIZ to attach quiz.");

        var existingQuiz = await _unitOfWork.Quizs.GetByConditionAsync(q => q.LessonID == request.LessonID);
        if (existingQuiz != null)
            throw new ValidationException("This lesson already has a quiz.");

        var quiz = _mapper.Map<Quiz>(request);
        quiz.QuizID = Guid.NewGuid();
        quiz.CreateAt = _clock.Now;
        quiz.UpdateAt = _clock.Now;
        quiz.CreateBy = _currentUser.UserId;
        quiz.UpdateBy = _currentUser.UserId;

        await _unitOfWork.Quizs.AddAsync(quiz);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<QuizClientViewDTO>(quiz);
    }

    public async Task<IEnumerable<QuizClientViewDTO>> GetQuizzesAsync()
    {
        var quizzes = await _unitOfWork.Quizs.GetAllAsync(
            orderBy: q => q.OrderByDescending(x => x.CreateAt),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return _mapper.Map<IEnumerable<QuizClientViewDTO>>(quizzes.Data);
    }

    public async Task<QuizClientViewDTO> GetQuizByIdAsync(Guid quizId)
    {
        var quiz = await _unitOfWork.Quizs.GetByIdAsync(quizId);
        if (quiz == null)
            throw new BaseException("Quiz not found.", "NOT_FOUND");

        return _mapper.Map<QuizClientViewDTO>(quiz);
    }

    public async Task<QuizClientViewDTO> UpdateQuizAsync(Guid quizId, UpdateQuizRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        ValidateQuizData(request.TimeLimit, request.TotalScore, request.PassScore);

        var quiz = await _unitOfWork.Quizs.GetByIdAsync(quizId);
        if (quiz == null)
            throw new BaseException("Quiz not found.", "NOT_FOUND");

        _mapper.Map(request, quiz);
        quiz.UpdateAt = _clock.Now;
        quiz.UpdateBy = _currentUser.UserId;

        await _unitOfWork.Quizs.UpdateAsync(quiz);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<QuizClientViewDTO>(quiz);
    }

    public async Task DeleteQuizAsync(Guid quizId)
    {
        var quiz = await _unitOfWork.Quizs.GetByIdAsync(quizId);
        if (quiz == null)
            throw new BaseException("Quiz not found.", "NOT_FOUND");

        await _unitOfWork.Quizs.DeleteAsync(quiz);
        await _unitOfWork.SaveChangesAsync();
    }

    private static void ValidateQuizData(int timeLimit, float totalScore, float passScore)
    {
        if (timeLimit <= 0)
            throw new ValidationException("TimeLimit must be greater than 0.");

        if (totalScore <= 0)
            throw new ValidationException("TotalScore must be greater than 0.");

        if (passScore < 0)
            throw new ValidationException("PassScore must be greater than or equal to 0.");

        if (passScore > totalScore)
            throw new ValidationException("PassScore cannot be greater than TotalScore.");
    }
}
