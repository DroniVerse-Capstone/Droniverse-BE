using System.Linq.Expressions;

namespace Droniverse.Community.Domain.IRepository
{
    public interface IRepository<T> where T : class
    {
        public Task<T?> GetByCondition(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IQueryable<T>>? include = null);
        public Task<IEnumerable<T>> GetManyByCondition(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
        public Task<IEnumerable<T>> GetAll();
        public Task<T> Add(T entity);
        public Task<T?> Update(T entity);
        public Task Delete(T entity);
    }
}
