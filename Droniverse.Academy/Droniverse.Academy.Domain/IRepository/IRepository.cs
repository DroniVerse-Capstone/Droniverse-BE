using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Academy.Domain.IRepository
{
    public interface IRepository<T> where T : class
    {
        // Read
        Task<T?> GetByConditionAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

        // Read (paged)
        // Returns a PaginationResult containing the page of items and pagination metadata.
        // - filter: optional filter expression
        // - orderBy: optional ordering function applied to IQueryable<T>
        // - pageIndex/pageSize: pagination parameters (pageIndex is 1-based)
        // - includeProperties: optional comma-separated navigation properties to include
        Task<PaginationResult<IEnumerable<T>>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int pageIndex = 1,
            int pageSize = 10,
            string? includeProperties = null,
            CancellationToken cancellationToken = default);

        // Create
        Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

        // Update
        Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default);

        // Delete
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    }
}
