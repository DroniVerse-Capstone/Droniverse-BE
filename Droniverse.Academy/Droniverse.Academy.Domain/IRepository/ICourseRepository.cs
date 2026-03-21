using Droniverse.Academy.Domain.Entities;
using Droniverse.Shared.DTOs.Response;
using System.Linq.Expressions;

namespace Droniverse.Academy.Domain.IRepository;
public interface ICourseRepository : IRepository<Course>
{
        /// <summary>
        /// Get course by id including current version only.
        /// Use for course APIs.
        /// </summary>
        Task<Course?> GetByIdWithCurrentVersionAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get course by id including ALL versions.
        /// Use for admin/instructor management.
        /// </summary>
        Task<Course?> GetByIdWithAllVersionsAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paginated courses including current version only.
        /// Use for course APIs.
        /// </summary>
        Task<PaginationResult<IEnumerable<Course>>>
            GetAllWithCurrentVersionAsync(
                Expression<Func<Course, bool>>? filter = null,
                Func<IQueryable<Course>, IOrderedQueryable<Course>>? orderBy = null,
                int pageIndex = 1,
                int pageSize = 10,
                CancellationToken cancellationToken = default);


        /// <summary>
        /// Get paginated courses including ALL versions.
        /// Use for admin portal/reporting.
        /// </summary>
        Task<PaginationResult<IEnumerable<Course>>>
            GetAllWithAllVersionsAsync(
                Expression<Func<Course, bool>>? filter = null,
                Func<IQueryable<Course>, IOrderedQueryable<Course>>? orderBy = null,
                int pageIndex = 1,
                int pageSize = 10,
                CancellationToken cancellationToken = default);


}

