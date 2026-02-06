using Droniverse.Shared.DTOs.Response;
using System.Linq.Expressions;

public interface IRepository<T> where T : class
{
    // Fast path - PK only, no Include
    Task<T?> GetByIdAsync(
        object id,
        CancellationToken cancellationToken = default
    );

    // Read by condition
    Task<T?> GetByConditionAsync(
        Expression<Func<T, bool>> predicate,
        string? includeProperties = null,
        CancellationToken cancellationToken = default
    );


    // Read (paged)
    Task<PaginationResult<IEnumerable<T>>> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int pageIndex = 1,
        int pageSize = 10,
        string? includeProperties = null,
        CancellationToken cancellationToken = default
    );

    // Create / Update / Delete
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
