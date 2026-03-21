using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.Validators;
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

        QuizValidator.ValidateQuizData(request.TimeLimit, request.TotalScore, request.PassScore);

        var lesson = await _unitOfWork.Lessons.GetByIdAsync(request.LessonID);
        if (lesson == null)
            throw new BaseException("Không tìm thấy bài học.", "NOT_FOUND");

        if (lesson.Type != LessonType.QUIZ)
            throw new ValidationException("Loại bài học phải là QUIZ để gắn bài kiểm tra.");

        if (lesson.ReferenceID != Guid.Empty)
            throw new ValidationException("Bài học này đã có bài kiểm tra.");

        var quiz = _mapper.Map<Quiz>(request);
        quiz.QuizID = Guid.NewGuid();
        quiz.CreateAt = _clock.Now;
        quiz.UpdateAt = _clock.Now;
        quiz.CreateBy = _currentUser.UserId;
        quiz.UpdateBy = _currentUser.UserId;

        await _unitOfWork.Quizs.AddAsync(quiz);

        lesson.ReferenceID = quiz.QuizID;
        await _unitOfWork.Lessons.UpdateAsync(lesson);

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
            throw new BaseException("Không tìm thấy bài kiểm tra.", "NOT_FOUND");

        return _mapper.Map<QuizClientViewDTO>(quiz);
    }

    public async Task<QuizClientViewDTO> UpdateQuizAsync(Guid quizId, UpdateQuizRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        QuizValidator.ValidateQuizData(request.TimeLimit, request.TotalScore, request.PassScore);

        var quiz = await _unitOfWork.Quizs.GetByIdAsync(quizId);
        if (quiz == null)
            throw new BaseException("Không tìm thấy bài kiểm tra.", "NOT_FOUND");

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
            throw new BaseException("Không tìm thấy bài kiểm tra.", "NOT_FOUND");

        var lesson = await _unitOfWork.Lessons.GetByConditionAsync(l => l.Type == LessonType.QUIZ && l.ReferenceID == quiz.QuizID);
        if (lesson != null && lesson.ReferenceID == quiz.QuizID)
        {
            lesson.ReferenceID = Guid.Empty;
            await _unitOfWork.Lessons.UpdateAsync(lesson);
        }

        await _unitOfWork.Quizs.DeleteAsync(quiz);
        await _unitOfWork.SaveChangesAsync();
    }
}
