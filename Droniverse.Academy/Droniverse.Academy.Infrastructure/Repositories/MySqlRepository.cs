using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Droniverse.Shared.DTOs.Response;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories
{
    internal class MySqlRepository<T> : IRepository<T> where T : class
    {
        protected readonly MySqlDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public MySqlRepository(MySqlDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public virtual async Task<T?> GetByConditionAsync(Expression<Func<T, bool>> predicate, string? includeProperties = null, CancellationToken cancellationToken = default)
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                var includes = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                .Select(p => p.Trim());
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.Where(predicate).FirstOrDefaultAsync(cancellationToken);
        }

        public virtual async Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            // DbSet.FindAsync accepts a params object[] for key values
            var value = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            return value;
        }

        public virtual async Task<PaginationResult<IEnumerable<T>>> GetAllAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int pageIndex = 1,
            int pageSize = 10,
            string? includeProperties = null,
            CancellationToken cancellationToken = default)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 10;

            IQueryable<T> query = _dbSet.AsQueryable();

            if (filter is not null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                var includes = includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                .Select(p => p.Trim());
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            if (orderBy is not null)
            {
                query = orderBy(query);
            }

            var skip = (pageIndex - 1) * pageSize;
            var items = await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);

            var result = new PaginationResult<IEnumerable<T>>(items, totalRecords, pageIndex, pageSize);
            return result;
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual Task<T?> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(entity);
            return Task.FromResult<T?>(entity);
        }

        public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            _context.Remove(entity);
            return Task.CompletedTask;
        }
    }
}

