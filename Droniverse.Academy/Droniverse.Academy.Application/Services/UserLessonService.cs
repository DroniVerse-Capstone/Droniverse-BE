using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services;

namespace Droniverse.Academy.Application.Services;

public class UserLessonService : IUserLessonService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public UserLessonService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<UserLessonResponseDTO> CreateUserLessonAsync(CreateUserLessonRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Progress is < 0 or > 100)
            throw new ValidationException("Progress phải nằm trong khoảng từ 0 đến 100.");

        if (await _unitOfWork.Lessons.GetByIdAsync(request.LessonID) == null)
            throw new BaseException("Không tìm thấy lesson.", "NOT_FOUND");

        var userId = _currentUser.UserId;

        var existing = await _unitOfWork.UserLessons.GetByConditionAsync(
            x => x.LessonID == request.LessonID && x.UserID == userId);

        if (existing != null)
            throw new ValidationException("Người dùng đã có dữ liệu lesson này.");

        var userLesson = _mapper.Map<UserLesson>(request);
        userLesson.UserLessonID = Guid.NewGuid();
        userLesson.UserID = userId;
        userLesson.LastAccessDate = _clock.Now;

        await _unitOfWork.UserLessons.AddAsync(userLesson);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserLessonResponseDTO>(userLesson);
    }

    public async Task<PaginationResult<IEnumerable<UserLessonResponseDTO>>> GetMyUserLessonsAsync(int pageIndex = 1, int pageSize = 10, UserLessonStatus? status = null)
    {
        var userId = _currentUser.UserId;

        var result = await _unitOfWork.UserLessons.GetAllAsync(
            status.HasValue
                ? x => x.UserID == userId && x.Status == status.Value
                : x => x.UserID == userId,
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.LastAccessDate));

        var mapped = result.Data.Select(x => _mapper.Map<UserLessonResponseDTO>(x)).ToList();
        return new PaginationResult<IEnumerable<UserLessonResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<UserLessonResponseDTO> GetMyUserLessonByIdAsync(Guid userLessonId)
    {
        var userId = _currentUser.UserId;

        var userLesson = await _unitOfWork.UserLessons.GetByConditionAsync(
            x => x.UserLessonID == userLessonId && x.UserID == userId);

        if (userLesson == null)
            throw new BaseException("Không tìm thấy user lesson.", "NOT_FOUND");

        return _mapper.Map<UserLessonResponseDTO>(userLesson);
    }

    public async Task<UserLessonResponseDTO> UpdateMyUserLessonAsync(Guid userLessonId, UpdateUserLessonRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Progress.HasValue && request.Progress is < 0 or > 100)
            throw new ValidationException("Progress phải nằm trong khoảng từ 0 đến 100.");

        var userId = _currentUser.UserId;

        var userLesson = await _unitOfWork.UserLessons.GetByConditionAsync(
            x => x.UserLessonID == userLessonId && x.UserID == userId);

        if (userLesson == null)
            throw new BaseException("Không tìm thấy user lesson.", "NOT_FOUND");

        if (request.Status.HasValue)
            userLesson.Status = request.Status.Value;

        if (request.Progress.HasValue)
            userLesson.Progress = request.Progress.Value;

        userLesson.LastAccessDate = request.LastAccessDate ?? _clock.Now;

        await _unitOfWork.UserLessons.UpdateAsync(userLesson);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserLessonResponseDTO>(userLesson);
    }

    public async Task DeleteMyUserLessonAsync(Guid userLessonId)
    {
        var userId = _currentUser.UserId;

        var userLesson = await _unitOfWork.UserLessons.GetByConditionAsync(
            x => x.UserLessonID == userLessonId && x.UserID == userId);

        if (userLesson == null)
            throw new BaseException("Không tìm thấy user lesson.", "NOT_FOUND");

        await _unitOfWork.UserLessons.DeleteAsync(userLesson);
        await _unitOfWork.SaveChangesAsync();
    }
}
