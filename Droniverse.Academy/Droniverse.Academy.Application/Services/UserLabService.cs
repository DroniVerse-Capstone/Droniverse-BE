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

public class UserLabService : IUserLabService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public UserLabService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<UserLabResponseDTO> CreateUserLabAsync(CreateUserLabRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (await _unitOfWork.Labs.GetByIdAsync(request.LabID) == null)
            throw new BaseException("Không tìm thấy lab.", "NOT_FOUND");

        var userId = _currentUser.UserId;

        var userLab = _mapper.Map<UserLab>(request);
        userLab.UserLabID = Guid.NewGuid();
        userLab.UserID = userId;

        await _unitOfWork.UserLabs.AddAsync(userLab);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserLabResponseDTO>(userLab);
    }

    public async Task<PaginationResult<IEnumerable<UserLabResponseDTO>>> GetMyUserLabsAsync(int pageIndex = 1, int pageSize = 10, bool? isCompleted = null)
    {
        var userId = _currentUser.UserId;

        var result = await _unitOfWork.UserLabs.GetAllAsync(
            isCompleted.HasValue
                ? x => x.UserID == userId && x.IsCompleted == isCompleted.Value
                : x => x.UserID == userId,
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderByDescending(x => x.Point));

        var mapped = result.Data.Select(x => _mapper.Map<UserLabResponseDTO>(x)).ToList();
        return new PaginationResult<IEnumerable<UserLabResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<UserLabResponseDTO> GetMyUserLabByIdAsync(Guid userLabId)
    {
        var userId = _currentUser.UserId;

        var userLab = await _unitOfWork.UserLabs.GetByConditionAsync(
            x => x.UserLabID == userLabId && x.UserID == userId);

        if (userLab == null)
            throw new BaseException("Không tìm thấy user lab.", "NOT_FOUND");

        return _mapper.Map<UserLabResponseDTO>(userLab);
    }

    public async Task<UserLabResponseDTO> UpdateMyUserLabAsync(Guid userLabId, UpdateUserLabRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var userId = _currentUser.UserId;

        var userLab = await _unitOfWork.UserLabs.GetByConditionAsync(
            x => x.UserLabID == userLabId && x.UserID == userId);

        if (userLab == null)
            throw new BaseException("Không tìm thấy user lab.", "NOT_FOUND");

        userLab.Solution = request.Solution;
        userLab.IsCompleted = request.IsCompleted;
        userLab.Time = request.Time;
        userLab.NumberOfStep = request.NumberOfStep;
        userLab.Length = request.Length;
        userLab.FeedbackVN = request.FeedbackVN;
        userLab.FeedbackEN = request.FeedbackEN;
        userLab.Point = request.Point;

        await _unitOfWork.UserLabs.UpdateAsync(userLab);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserLabResponseDTO>(userLab);
    }

    public async Task DeleteMyUserLabAsync(Guid userLabId)
    {
        var userId = _currentUser.UserId;

        var userLab = await _unitOfWork.UserLabs.GetByConditionAsync(
            x => x.UserLabID == userLabId && x.UserID == userId);

        if (userLab == null)
            throw new BaseException("Không tìm thấy user lab.", "NOT_FOUND");

        await _unitOfWork.UserLabs.DeleteAsync(userLab);
        await _unitOfWork.SaveChangesAsync();
    }
}
