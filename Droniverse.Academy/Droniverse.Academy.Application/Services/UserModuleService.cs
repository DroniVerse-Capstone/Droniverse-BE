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

public class UserModuleService : IUserModuleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public UserModuleService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<UserModuleResponseDTO> CreateUserModuleAsync(CreateUserModuleRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Progress is < 0 or > 100)
            throw new ValidationException("Progress phải nằm trong khoảng từ 0 đến 100.");

        if (await _unitOfWork.Modules.GetByIdAsync(request.ModuleID) == null)
            throw new BaseException("Không tìm thấy module.", "NOT_FOUND");

        var userId = _currentUser.UserId;

        var existing = await _unitOfWork.UserModules.GetByConditionAsync(
            x => x.ModuleID == request.ModuleID && x.UserID == userId);

        if (existing != null)
            throw new ValidationException("Người dùng đã có dữ liệu module này.");

        var userModule = _mapper.Map<UserModule>(request);
        userModule.UserID = userId;
        userModule.EnrollDate = _clock.Now;
        userModule.CompleteDate = request.IsCompleted ? _clock.Now : null;

        await _unitOfWork.UserModules.AddAsync(userModule);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserModuleResponseDTO>(userModule);
    }

    public async Task<PaginationResult<IEnumerable<UserModuleResponseDTO>>> GetMyUserModulesAsync(int pageIndex = 1, int pageSize = 10, bool? isCompleted = null)
    {
        var userId = _currentUser.UserId;

        var result = await _unitOfWork.UserModules.GetAllAsync(
            isCompleted.HasValue
                ? x => x.UserID == userId && x.IsCompleted == isCompleted.Value
                : x => x.UserID == userId,
            pageIndex: pageIndex,
            pageSize: pageSize,
            orderBy: q => q.OrderBy(x => x.ModuleID));

        var mapped = result.Data.Select(x => _mapper.Map<UserModuleResponseDTO>(x)).ToList();
        return new PaginationResult<IEnumerable<UserModuleResponseDTO>>(mapped, result.TotalRecords, result.PageIndex, result.PageSize);
    }

    public async Task<UserModuleResponseDTO> GetMyUserModuleAsync(Guid moduleId)
    {
        var userId = _currentUser.UserId;

        var userModule = await _unitOfWork.UserModules.GetByConditionAsync(
            x => x.ModuleID == moduleId && x.UserID == userId);

        if (userModule == null)
            throw new BaseException("Không tìm thấy user module.", "NOT_FOUND");

        return _mapper.Map<UserModuleResponseDTO>(userModule);
    }

    public async Task<UserModuleResponseDTO> UpdateMyUserModuleAsync(Guid moduleId, UpdateUserModuleRequestDTO request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Progress is < 0 or > 100)
            throw new ValidationException("Progress phải nằm trong khoảng từ 0 đến 100.");

        var userId = _currentUser.UserId;

        var userModule = await _unitOfWork.UserModules.GetByConditionAsync(
            x => x.ModuleID == moduleId && x.UserID == userId);

        if (userModule == null)
            throw new BaseException("Không tìm thấy user module.", "NOT_FOUND");

        userModule.Progress = request.Progress;
        userModule.IsCompleted = request.IsCompleted;
        userModule.CompleteDate = request.IsCompleted ? _clock.Now : null;

        await _unitOfWork.UserModules.UpdateAsync(userModule);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserModuleResponseDTO>(userModule);
    }

    public async Task DeleteMyUserModuleAsync(Guid moduleId)
    {
        var userId = _currentUser.UserId;

        var userModule = await _unitOfWork.UserModules.GetByConditionAsync(
            x => x.ModuleID == moduleId && x.UserID == userId);

        if (userModule == null)
            throw new BaseException("Không tìm thấy user module.", "NOT_FOUND");

        await _unitOfWork.UserModules.DeleteAsync(userModule);
        await _unitOfWork.SaveChangesAsync();
    }
}
