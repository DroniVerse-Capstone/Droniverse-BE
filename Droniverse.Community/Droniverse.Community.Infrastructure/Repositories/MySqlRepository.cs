using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Droniverse.Community.Infrastructure.Repositories;
public class MySqlRepository<T> : IRepository<T> where T : class
{
    protected readonly DbSet<T> _dbSet;
    protected readonly MySqlDbContext _context;
    public MySqlRepository(MySqlDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    public async Task<T> Add(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public Task Delete(T entity)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<T>> GetAll()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByCondition(Expression<Func<T, bool>> expression)
    {
        return await _dbSet.Where(expression).FirstOrDefaultAsync();
    }

    public async Task<T?> Update(T entity)
    {
        //_dbSet.Entry(entity).State = EntityState.Modified;
        _dbSet.Update(entity);
        return await Task.FromResult(entity);
    }
}
