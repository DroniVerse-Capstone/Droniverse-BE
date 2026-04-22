using AutoMapper;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Droniverse.Shared.Exceptions;

namespace Droniverse.Academy.Application.Services
{
    public class LevelService : ILevelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<LevelService> _logger;
        private readonly IMapper _mapper;

        public LevelService(IUnitOfWork unitOfWork, ILogger<LevelService> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }
        public async Task<IEnumerable<LevelMiniResponse>> GetLevelByDroneAsync(Guid droneId)
        {
            var levels = await _unitOfWork.Levels.GetAllAsync(
            filter: l => l.DroneID == droneId,
            orderBy: q => q.OrderBy(l => l.LevelNumber),
            pageIndex: 1,
            pageSize: int.MaxValue);
            return _mapper.Map<IEnumerable<LevelMiniResponse>>(levels.Data);
        }

        public async Task<IEnumerable<LevelPathResponseDTO>> GetLevelPathAsync(Guid droneId, CancellationToken cancellationToken = default)
        {
            var levelsResult = await _unitOfWork.Levels.GetAllAsync(
                filter: l => l.DroneID == droneId && l.LevelNumber >= 1 && l.LevelNumber <= 4,
                orderBy: q => q.OrderBy(l => l.LevelNumber),
                pageIndex: 1,
                pageSize: int.MaxValue,
                includeProperties: "LevelCourseRequirements.Course.CurrentVersion,LevelCourseRequirements.Course.Level",
                cancellationToken: cancellationToken);

            var levels = levelsResult.Data?.ToList() ?? [];

            return levels.Select(level => new LevelPathResponseDTO
            {
                Level = _mapper.Map<LevelMiniResponse>(level),
                Courses = level.LevelCourseRequirements?
                    .Select(x => x.Course)
                    .Where(course => course != null)
                    .Select(course => new CourseMiniResponseDTO
                    {
                        CourseID = course!.CourseID,
                        Level = course.Level == null ? null : _mapper.Map<LevelMiniResponse>(course.Level),
                        CurrentVersion = course.CurrentVersion == null ? null : new CourseVersionMiniResponseDTO
                        {
                            CourseVersionID = course.CurrentVersion.CourseVersionID,
                            TitleVN = course.CurrentVersion.TitleVN,
                            TitleEN = course.CurrentVersion.TitleEN,
                            Version = course.CurrentVersion.Version
                        }
                    })
                    .OrderBy(x => x.Level?.LevelNumber)
                    .ThenBy(x => x.CurrentVersion?.Version)
                    .ToList() ?? []
            }).ToList();
        }

        public async Task<int> ReplaceLevelCoursesAsync(Guid levelId, IEnumerable<Guid>? courseIds, CancellationToken cancellationToken = default)
        {
            if (levelId == Guid.Empty)
                throw new ValidationException("Level không hợp lệ.");

            var level = await _unitOfWork.Levels.GetByIdAsync(levelId, cancellationToken);
            if (level == null)
                throw new BaseException("Không tìm thấy level.", "NOT_FOUND");

            var ids = courseIds?
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList() ?? [];

            if (ids.Count == 0)
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existing = await _unitOfWork.LevelCourseRequirements.GetByLevelIdAsync(levelId, cancellationToken);
                    if (existing.Count > 0)
                    {
                        _unitOfWork.LevelCourseRequirements.RemoveRange(existing);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    _logger.LogInformation("Cleared level course requirements for Level {LevelId}.", levelId);
                    return 0;
                });
            }

            var coursesResult = await _unitOfWork.Courses.GetAllAsync(
                filter: c => ids.Contains(c.CourseID),
                includeProperties: "Level",
                pageIndex: 1,
                pageSize: int.MaxValue,
                cancellationToken: cancellationToken);

            var courses = coursesResult.Data?.ToList() ?? [];
            var courseLookup = courses.ToDictionary(x => x.CourseID);

            var missingIds = ids.Where(id => !courseLookup.ContainsKey(id)).ToList();
            if (missingIds.Count > 0)
            {
                throw new ValidationException("Có course không tồn tại.");
            }

            foreach (var courseId in ids)
            {
                var course = courseLookup[courseId];

                if (course.LevelID == null || course.Level == null)
                    throw new ValidationException($"Course {courseId} chưa được gán level.");

                if (course.Level.DroneID != level.DroneID)
                    throw new ValidationException($"Course {courseId} không cùng drone với level mục tiêu.");

                if (course.Level.LevelNumber != level.LevelNumber - 1)
                    throw new ValidationException($"Course {courseId} phải thuộc level ngay trước level mục tiêu.");
            }

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existing = await _unitOfWork.LevelCourseRequirements.GetByLevelIdAsync(levelId, cancellationToken);
                if (existing.Count > 0)
                {
                    _unitOfWork.LevelCourseRequirements.RemoveRange(existing);
                }

                var newEntities = ids.Select(courseId => new LevelCourseRequirement
                {
                    LevelID = levelId,
                    CourseID = courseId
                }).ToList();

                await _unitOfWork.LevelCourseRequirements.AddRangeAsync(newEntities, cancellationToken);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Replaced level course requirements for Level {LevelId}. Count = {Count}", levelId, ids.Count);

                return ids.Count;
            });
        }
    }
}
