using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Domain.Models;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CourseVersionRepository : MySqlRepository<CourseVersion>, ICourseVersionRepository
{
    public CourseVersionRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<CourseOverviewData?> GetCourseOverviewDataAsync(
        Guid courseVersionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(cv => cv.CourseVersionID == courseVersionId)
            .Select(cv => new CourseOverviewData
            {
                AuthorId = cv.Course.CreateBy,
                CourseID = cv.CourseID,
                CourseVersionID = cv.CourseVersionID,
                TitleVN = cv.TitleVN,
                TitleEN = cv.TitleEN,
                DescriptionVN = cv.DescriptionVN,
                DescriptionEN = cv.DescriptionEN,
                ContextVN = cv.ContextVN,
                ContextEN = cv.ContextEN,
                ImageUrl = cv.ImageUrl,
                EstimatedDuration = cv.EstimatedDuration,
                AverageRating = cv.Feedbacks
                    .Select(f => (decimal?)f.Rating)
                    .Average() ?? 0m,
                TotalFeedback = cv.Feedbacks.Count(),
                TotalLearners = cv.Enrollments.Count(),
                TotalModules = cv.Modules.Count(),
                TotalTheory = cv.Modules
                    .SelectMany(m => m.Lessons)
                    .Count(l => l.Type == LessonType.THEORY),
                TotalQuiz = cv.Modules
                    .SelectMany(m => m.Lessons)
                    .Count(l => l.Type == LessonType.QUIZ),
                TotalLab = cv.Modules
                    .SelectMany(m => m.Lessons)
                    .Count(l => l.Type == LessonType.LAB),
                CertificateImageUrl = cv.Certificate != null
                    ? cv.Certificate.ImageUrl
                    : null,
                IsUnlock = cv.Course.Codes
                    .Any(c => c.UsedByUserID == userId && c.Status == CodeStatus.USED),
                LastUpdatedById = cv.UpdateBy,
                LastUpdatedAt = cv.UpdateAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

