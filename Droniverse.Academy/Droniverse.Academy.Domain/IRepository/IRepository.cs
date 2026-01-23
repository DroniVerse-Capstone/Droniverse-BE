using System.Linq.Expressions;
namespace Droniverse.Academy.Domain.IRepository
{
    public interface IRepository<T> where T : class
    {
        public Task<T?> GetByCondition(Expression<Func<T, bool>> expression);
        public Task<IEnumerable<T>> GetAll();
        public Task<T> Add(T entity);
        public Task<T?> Update(T entity);
        public Task Delete(T entity);
    }
}