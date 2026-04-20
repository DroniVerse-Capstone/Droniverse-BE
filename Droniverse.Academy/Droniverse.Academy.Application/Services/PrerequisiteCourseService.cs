using AutoMapper;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Academy.Application.Services
{
    public class PrerequisiteCourseService : IPrerequisiteCourseService
    {
        private readonly ILogger<PrerequisiteCourseService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PrerequisiteCourseService(ILogger<PrerequisiteCourseService> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public Task<int> ReplacePrerequisitesAsync(Guid courseId, IEnumerable<Guid> prerequisiteCourseIds, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        //public async Task<int> ReplacePrerequisitesAsync(
        //    Guid courseId,
        //    IEnumerable<Guid> prerequisiteCourseIds,
        //    CancellationToken cancellationToken = default)
        //{
        //    if (courseId == Guid.Empty)
        //        throw new ArgumentException("courseId không hợp lệ.");

        //    if (prerequisiteCourseIds == null)
        //        throw new ArgumentNullException(nameof(prerequisiteCourseIds));

        //    var ids = prerequisiteCourseIds
        //        .Where(x => x != Guid.Empty)
        //        .Distinct()
        //        .ToList();

        //    // Bắt đầu transaction
        //    using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        //    try
        //    {
        //        // 1. Lấy toàn bộ prerequisite hiện tại
        //        var existing = await _unitOfWork.PrerequisiteCourseRepository
        //            .GetByCourseIdAsync(courseId, cancellationToken);

        //        // 2. Xóa toàn bộ
        //        if (existing.Any())
        //        {
        //            _unitOfWork.PrerequisiteCourseRepository.RemoveRange(existing);
        //        }

        //        // 3. Tạo mới
        //        var newEntities = ids.Select(id => new Domain.Entities.PrerequisiteCourse
        //        {
        //            CourseID = courseId,
        //            PrerequisiteCourseID = id
        //        }).ToList();

        //        if (newEntities.Any())
        //        {
        //            await _unitOfWork.PrerequisiteCourseRepository
        //                .AddRangeAsync(newEntities, cancellationToken);
        //        }

        //        // 4. Save
        //        await _unitOfWork.SaveChangesAsync(cancellationToken);

        //        // 5. Commit
        //        await transaction.CommitAsync(cancellationToken);

        //        _logger.LogInformation("Replaced prerequisites for Course {CourseId}. Count = {Count}", courseId, newEntities.Count);

        //        return newEntities.Count;
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync(cancellationToken);

        //        _logger.LogError(ex, "Error replacing prerequisites for Course {CourseId}", courseId);

        //        throw;
        //    }
        //}
    }
}
