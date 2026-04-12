using AutoMapper;
using Droniverse.Academy.Application.Common.Extensions;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.Validators;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class QuizService : IQuizService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;
    private readonly IUserDisplayNameService _userDisplayNameService;

    public QuizService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IClock clock, IUserDisplayNameService userDisplayNameService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
        _userDisplayNameService = userDisplayNameService;
    }

    public async Task<QuizClientViewDTO> CreateQuizAsync(CreateQuizRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        QuizValidator.ValidateQuizData(request.TimeLimit, request.TotalScore, request.PassScore);

        var module = await _unitOfWork.Modules.GetByIdAsync(request.ModuleID);
        if (module == null)
            throw new BaseException("Không tìm thấy mô-đun.", "NOT_FOUND");

        var orderIndex = request.OrderIndex ?? await GetNextOrderIndexAsync(request.ModuleID);
        await ValidateOrderIndexAsync(request.ModuleID, orderIndex);

        var quiz = _mapper.Map<Quiz>(request);
        quiz.QuizID = Guid.NewGuid();
        quiz.SetAuditOnCreate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Quizs.AddAsync(quiz);

        var lesson = new Lesson
        {
            LessonID = Guid.NewGuid(),
            ModuleID = request.ModuleID,
            OrderIndex = orderIndex,
            Type = LessonType.QUIZ,
            ReferenceID = quiz.QuizID
        };

        await _unitOfWork.Lessons.AddAsync(lesson);

        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<QuizClientViewDTO>(quiz);
        await PopulateUsersAsync(response, quiz.CreateBy, quiz.UpdateBy);

        return response;
    }

    public async Task<IEnumerable<QuizClientViewDTO>> GetQuizzesAsync()
    {
        var quizzes = await _unitOfWork.Quizs.GetAllAsync(
            orderBy: q => q.OrderByDescending(x => x.CreateAt),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var entities = quizzes.Data.ToList();
        var mapped = _mapper.Map<List<QuizClientViewDTO>>(entities);

        var userLookup = await BuildUserLookupAsync(entities);
        PopulateMappedQuizzesUsers(entities, mapped, userLookup);

        return mapped;
    }

    public async Task<QuizClientViewDTO> GetQuizByIdAsync(Guid quizId)
    {
        var quiz = await _unitOfWork.Quizs.GetByIdAsync(quizId);
        if (quiz == null)
            throw new BaseException("Không tìm thấy bài kiểm tra.", "NOT_FOUND");

        var response = _mapper.Map<QuizClientViewDTO>(quiz);
        await PopulateUsersAsync(response, quiz.CreateBy, quiz.UpdateBy);

        return response;
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
        quiz.SetAuditOnUpdate(_currentUser.UserId, _clock.Now);

        await _unitOfWork.Quizs.UpdateAsync(quiz);
        await _unitOfWork.SaveChangesAsync();

        var response = _mapper.Map<QuizClientViewDTO>(quiz);
        await PopulateUsersAsync(response, quiz.CreateBy, quiz.UpdateBy);

        return response;
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

    private async Task<int> GetNextOrderIndexAsync(Guid moduleId)
    {
        var lessons = await _unitOfWork.Lessons.GetAllAsync(
            filter: l => l.ModuleID == moduleId,
            orderBy: q => q.OrderByDescending(l => l.OrderIndex),
            pageIndex: 1,
            pageSize: 1);

        var latest = lessons.Data.FirstOrDefault();
        return (latest?.OrderIndex ?? 0) + 1;
    }

    private async Task ValidateOrderIndexAsync(Guid moduleId, int orderIndex)
    {
        if (orderIndex <= 0)
            throw new ValidationException("OrderIndex phải lớn hơn 0.");

        var duplicated = await _unitOfWork.Lessons.GetByConditionAsync(
            l => l.ModuleID == moduleId && l.OrderIndex == orderIndex);

        if (duplicated != null)
            throw new ValidationException("OrderIndex phải là duy nhất trong mô-đun.");
    }

    private async Task PopulateUsersAsync(QuizClientViewDTO quiz, Guid createBy, Guid updateBy)
    {
        var users = await _userDisplayNameService.GetListUserAsync(new[] { createBy, updateBy });
        var userLookup = users.ToDictionary(u => u.UserId, u => (SimpleUserReponse?)u);

        userLookup.TryGetValue(createBy, out var creator);
        userLookup.TryGetValue(updateBy, out var updater);

        quiz.Creator = creator;
        quiz.Updater = updater;
    }

    private async Task<Dictionary<Guid, SimpleUserReponse?>> BuildUserLookupAsync(IEnumerable<Quiz> quizzes)
    {
        var userIds = quizzes
            .SelectMany(q => new[] { q.CreateBy, q.UpdateBy })
            .ToDistinctValidIds();

        var users = await _userDisplayNameService.GetListUserAsync(userIds);
        var lookup = users.ToDictionary(u => u.UserId, u => (SimpleUserReponse?)u);

        foreach (var userId in userIds)
        {
            lookup.TryAdd(userId, null);
        }

        return lookup;
    }

    private static void PopulateMappedQuizzesUsers(
        IEnumerable<Quiz> entities,
        IEnumerable<QuizClientViewDTO> dtos,
        IReadOnlyDictionary<Guid, SimpleUserReponse?> userLookup)
    {
        foreach (var (entity, dto) in entities.Zip(dtos))
        {
            if (userLookup.TryGetValue(entity.CreateBy, out var creator))
            {
                dto.Creator = creator;
            }

            if (userLookup.TryGetValue(entity.UpdateBy, out var updater))
            {
                dto.Updater = updater;
            }
        }
    }
}
