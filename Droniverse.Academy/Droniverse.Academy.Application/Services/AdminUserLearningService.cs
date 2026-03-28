using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Application.Services;

public class AdminUserLearningService : IAdminUserLearningService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AdminUserLearningService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginationResult<IEnumerable<UserLabResponseDTO>>> GetUserLabsAsync(Guid userId, int pageIndex = 1, int pageSize = 10, bool? isCompleted = null)
    {
        var result = await _unitOfWork.UserLabs.GetAllAsync(
            filter: x => x.UserID == userId && (!isCompleted.HasValue || x.IsCompleted == isCompleted.Value),
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.Point));

        var mapped = _mapper.Map<IEnumerable<UserLabResponseDTO>>(result.Data);
        return new PaginationResult<IEnumerable<UserLabResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<PaginationResult<IEnumerable<UserQuizAttemptResponseDTO>>> GetUserQuizAttemptsAsync(Guid userId, int pageIndex = 1, int pageSize = 10, bool? isPassed = null)
    {
        var result = await _unitOfWork.QuizAttempts.GetAllAsync(
            filter: x => x.UserID == userId && (!isPassed.HasValue || x.IsPassed == isPassed.Value),
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.StartTime));

        var mapped = _mapper.Map<IEnumerable<UserQuizAttemptResponseDTO>>(result.Data);
        return new PaginationResult<IEnumerable<UserQuizAttemptResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<PaginationResult<IEnumerable<UserQuizQuestionAttemptResponseDTO>>> GetUserQuizQuestionAttemptsAsync(Guid userId, int pageIndex = 1, int pageSize = 10, bool? isCorrect = null)
    {
        var attempts = await _unitOfWork.QuizAttempts.GetAllAsync(
            filter: x => x.UserID == userId,
            pageIndex: 1,
            pageSize: int.MaxValue);

        var attemptIds = attempts.Data.Select(x => x.AttemptID).ToList();
        if (attemptIds.Count == 0)
            return new PaginationResult<IEnumerable<UserQuizQuestionAttemptResponseDTO>>(new List<UserQuizQuestionAttemptResponseDTO>(), 0, pageIndex, pageSize);

        var result = await _unitOfWork.QuizQuestionAttempts.GetAllAsync(
            filter: x => attemptIds.Contains(x.AttemptID) && (!isCorrect.HasValue || x.IsCorrect == isCorrect.Value),
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.AttemptAnswerID));

        var mapped = _mapper.Map<IEnumerable<UserQuizQuestionAttemptResponseDTO>>(result.Data);
        return new PaginationResult<IEnumerable<UserQuizQuestionAttemptResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<PaginationResult<IEnumerable<UserModuleResponseDTO>>> GetUserModulesAsync(Guid userId, int pageIndex = 1, int pageSize = 10, bool? isCompleted = null)
    {
        var result = await _unitOfWork.UserModules.GetAllAsync(
            filter: x => x.UserID == userId && (!isCompleted.HasValue || x.IsCompleted == isCompleted.Value),
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderBy(x => x.ModuleID));

        var mapped = _mapper.Map<IEnumerable<UserModuleResponseDTO>>(result.Data);
        return new PaginationResult<IEnumerable<UserModuleResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<PaginationResult<IEnumerable<UserLessonResponseDTO>>> GetUserLessonsAsync(Guid userId, int pageIndex = 1, int pageSize = 10, UserLessonStatus? status = null)
    {
        var result = await _unitOfWork.UserLessons.GetAllAsync(
            filter: x => x.UserID == userId && (!status.HasValue || x.Status == status.Value),
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.LastAccessDate));

        var mapped = _mapper.Map<IEnumerable<UserLessonResponseDTO>>(result.Data);
        return new PaginationResult<IEnumerable<UserLessonResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<PaginationResult<IEnumerable<EnrollmentResponseDTO>>> GetUserEnrollmentsAsync(Guid userId, int pageIndex = 1, int pageSize = 10, EnrollStatus? status = null)
    {
        var result = await _unitOfWork.Enrollments.GetAllAsync(
            filter: x => x.UserID == userId && (!status.HasValue || x.Status == status.Value),
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.EnrollDate));

        var mapped = _mapper.Map<IEnumerable<EnrollmentResponseDTO>>(result.Data);
        return new PaginationResult<IEnumerable<EnrollmentResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }
}
