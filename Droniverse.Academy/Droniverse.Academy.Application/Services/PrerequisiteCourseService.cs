using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Microsoft.Extensions.Logging;

namespace Droniverse.Academy.Application.Services
{
    public class PrerequisiteCourseService : IPrerequisiteCourseService
    {
        private readonly ILogger<PrerequisiteCourseService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public PrerequisiteCourseService(ILogger<PrerequisiteCourseService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> ReplacePrerequisitesAsync(Guid courseId, IEnumerable<Guid>? prerequisiteCourseIds, CancellationToken cancellationToken = default)
        {
            if (courseId == Guid.Empty)
                throw new ArgumentException("courseId không hợp lệ.");

            var ids = prerequisiteCourseIds?
                .Where(x => x != Guid.Empty && x != courseId)
                .Distinct()
                .ToList() ?? [];

            try
            {
                return await _unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var existing = await _unitOfWork.PrerequisiteCourses
                        .GetByCourseIdAsync(courseId, cancellationToken);

                    if (existing.Count > 0)
                    {
                        _unitOfWork.PrerequisiteCourses.RemoveRange(existing);
                    }

                    if (ids.Count > 0)
                    {
                        var newEntities = ids.Select(id => new PrerequisiteCourse
                        {
                            CourseID = courseId,
                            PrerequisiteCourseID = id
                        }).ToList();

                        await _unitOfWork.PrerequisiteCourses.AddRangeAsync(newEntities, cancellationToken);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Replaced prerequisites for Course {CourseId}. Count = {Count}", courseId, ids.Count);

                    return ids.Count;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error replacing prerequisites for Course {CourseId}", courseId);
                throw;
            }
        }
    }
}
