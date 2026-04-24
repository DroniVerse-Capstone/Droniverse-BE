using AutoMapper;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Exceptions;
using Droniverse.Shared.Services.IServices;

namespace Droniverse.Academy.Application.Services;

public class UserLevelService : IUserLevelService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IClock _clock;

    public UserLevelService(IUnitOfWork unitOfWork, IMapper mapper, IClock clock)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _clock = clock;
    }

    public async Task<IEnumerable<UserLevelResponse>> GetUserLevelsAsync(Guid userId)
    {
        var levels = await _unitOfWork.Levels.GetAllAsync(
            filter: l => l.UserLevels.Any(ul => ul.UserID == userId),
            orderBy: q => q.OrderBy(l => l.DroneID).ThenBy(l => l.LevelNumber),
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "Drone");

        return levels.Data.Select(level => new UserLevelResponse
        {
            UserID = userId,
            Level = _mapper.Map<LevelMiniResponse>(level),
            Drone = _mapper.Map<DroneMiniResponse>(level.Drone)
        });
    }

    public async Task<IEnumerable<UserLevelResponse>> GetMaxUserLevelsAsync(Guid userId)
    {
        var levels = await _unitOfWork.Levels.GetAllAsync(
            filter: l => l.UserLevels.Any(ul => ul.UserID == userId),
            orderBy: q => q.OrderBy(l => l.DroneID).ThenByDescending(l => l.LevelNumber),
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "Drone");

        var maxLevels = levels.Data
            .GroupBy(l => l.DroneID)
            .Select(g => g.OrderByDescending(x => x.LevelNumber).First())
            .OrderBy(l => l.DroneID)
            .ToList();

        return maxLevels.Select(level => new UserLevelResponse
        {
            UserID = userId,
            Level = _mapper.Map<LevelMiniResponse>(level),
            Drone = _mapper.Map<DroneMiniResponse>(level.Drone)
        });
    }

    public async Task<bool> CreateLevelOneIfFirstEnrollmentAsync(Guid userId, Guid courseVersionId)
    {
        ValidateUserAndCourseVersion(userId, courseVersionId);

        var level = await ResolveLevelFromCourseVersionAsync(courseVersionId);

        if (level.LevelNumber != 1)
            return false;

        var existingAnyLevelInDrone = await _unitOfWork.UserLevels.GetByConditionAsync(
            x => x.UserID == userId && x.Level.DroneID == level.DroneID,
            includeProperties: "Level");

        if (existingAnyLevelInDrone != null)
            return false;

        var existingLevelOne = await _unitOfWork.UserLevels.GetByConditionAsync(
            x => x.UserID == userId && x.LevelID == level.LevelID);

        if (existingLevelOne != null)
            return false;

        await _unitOfWork.UserLevels.AddAsync(new UserLevel
        {
            UserLevelID = Guid.NewGuid(),
            UserID = userId,
            LevelID = level.LevelID,
            AchievedAt = _clock.Now
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CanUserUpgradeAsync(Guid userId, Guid droneId)
    {
        ValidateUserAndDrone(userId, droneId);

        var nextLevel = await GetNextLevelAsync(userId, droneId);
        if (nextLevel == null)
            return false;

        var requirements = await _unitOfWork.LevelCourseRequirements.GetAllAsync(
            filter: x => x.LevelID == nextLevel.LevelID,
            pageIndex: 1,
            pageSize: int.MaxValue);

        var requiredCourseIds = requirements.Data
            .Select(x => x.CourseID)
            .Distinct()
            .ToList();

        if (requiredCourseIds.Count == 0)
            return true;

        var completedEnrollments = await _unitOfWork.Enrollments.GetAllAsync(
            filter: x => x.UserID == userId
                         && x.Status == EnrollStatus.COMPLETED
                         && requiredCourseIds.Contains(x.CourseID),
            pageIndex: 1,
            pageSize: int.MaxValue);

        var completedCourseIds = completedEnrollments.Data
            .Select(x => x.CourseID)
            .Distinct()
            .ToHashSet();

        return requiredCourseIds.All(completedCourseIds.Contains);
    }

    public async Task<bool> UpgradeUserLevelAsync(Guid userId, Guid droneId)
    {
        ValidateUserAndDrone(userId, droneId);

        var canUpgrade = await CanUserUpgradeAsync(userId, droneId);
        if (!canUpgrade)
            return false;

        var nextLevel = await GetNextLevelAsync(userId, droneId);
        if (nextLevel == null)
            return false;

        var existing = await _unitOfWork.UserLevels.GetByConditionAsync(
            x => x.UserID == userId && x.LevelID == nextLevel.LevelID);

        if (existing != null)
            return false;

        await _unitOfWork.UserLevels.AddAsync(new UserLevel
        {
            UserLevelID = Guid.NewGuid(),
            UserID = userId,
            LevelID = nextLevel.LevelID,
            AchievedAt = _clock.Now
        });

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static void ValidateUserAndCourseVersion(Guid userId, Guid courseVersionId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("UserId không hợp lệ.");

        if (courseVersionId == Guid.Empty)
            throw new ValidationException("CourseVersionId không hợp lệ.");
    }

    private static void ValidateUserAndDrone(Guid userId, Guid droneId)
    {
        if (userId == Guid.Empty)
            throw new ValidationException("UserId không hợp lệ.");

        if (droneId == Guid.Empty)
            throw new ValidationException("DroneId không hợp lệ.");
    }

    private async Task<Level> ResolveLevelFromCourseVersionAsync(Guid courseVersionId)
    {
        var courseVersion = await _unitOfWork.CourseVersions.GetByConditionAsync(
            x => x.CourseVersionID == courseVersionId,
            includeProperties: "Course.Level");

        if (courseVersion?.Course == null)
            throw new NotFoundException("Không tìm thấy course version.");

        if (courseVersion.Course.Level == null)
            throw new ValidationException("Khóa học chưa được gán level.");

        return courseVersion.Course.Level;
    }

    private async Task<Level?> GetNextLevelAsync(Guid userId, Guid droneId)
    {
        var userLevels = await _unitOfWork.UserLevels.GetAllAsync(
            filter: x => x.UserID == userId && x.Level.DroneID == droneId,
            pageIndex: 1,
            pageSize: int.MaxValue,
            includeProperties: "Level");

        var currentLevelNumber = userLevels.Data
            .Select(x => x.Level.LevelNumber)
            .DefaultIfEmpty(0)
            .Max();

        var nextLevelNumber = currentLevelNumber + 1;

        return await _unitOfWork.Levels.GetByConditionAsync(
            x => x.DroneID == droneId && x.LevelNumber == nextLevelNumber);
    }
}
