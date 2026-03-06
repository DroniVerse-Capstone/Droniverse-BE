using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Droniverse.Identity.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbSet<T> _dbSet;
    protected readonly IdentityDbContext _context;
    public Repository(IdentityDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    public async Task<T> Add(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public Task<bool> Delete(T entity)
    {
        try
        {
            var a = _dbSet.Remove(entity);
            return Task.FromResult(true);
        }
        catch (Exception)
        {
            throw;
        }
        

    }

    public async Task<IEnumerable<T>> GetAll()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByCondition(Expression<Func<T, bool>> expression)
    {
        return await _dbSet.Where(expression).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetManyByCondition(Expression<Func<T, bool>> expression)
    {
        return await _dbSet.Where(expression).ToListAsync();
    }

    public async Task<T?> Update(T entity)
    {
        //_dbSet.Entry(entity).State = EntityState.Modified;
        _dbSet.Update(entity);
        return await Task.FromResult(entity);
    }
}
