using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.IRepository;

namespace Droniverse.Academy.Application.Services;

public class UserLevelService : IUserLevelService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserLevelService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LevelMiniResponse>> GetUserLevelsAsync(Guid userId)
    {
        var levels = await _unitOfWork.Levels.GetAllAsync(
            filter: l => l.UserLevels.Any(ul => ul.UserID == userId),
            orderBy: q => q.OrderBy(l => l.DroneID).ThenBy(l => l.LevelNumber),
            pageIndex: 1,
            pageSize: int.MaxValue);

        return _mapper.Map<IEnumerable<LevelMiniResponse>>(levels.Data);
    }

    public async Task<IEnumerable<LevelMiniResponse>> GetMaxUserLevelsAsync(Guid userId)
    {
        var levels = await _unitOfWork.Levels.GetAllAsync(
            filter: l => l.UserLevels.Any(ul => ul.UserID == userId),
            orderBy: q => q.OrderBy(l => l.DroneID).ThenByDescending(l => l.LevelNumber),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var maxLevels = levels.Data
            .GroupBy(l => l.DroneID)
            .Select(g => g.OrderByDescending(x => x.LevelNumber).First())
            .OrderBy(l => l.DroneID)
            .ToList();

        return _mapper.Map<IEnumerable<LevelMiniResponse>>(maxLevels);
    }
}
