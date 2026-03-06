using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Identity.Domain.Interfaces;
public interface IRepository<T> where T : class
{
    public Task<T?> GetByCondition(Expression<Func<T, bool>> expression);
    public Task<IEnumerable<T>> GetManyByCondition(Expression<Func<T, bool>> expression);
    public Task<IEnumerable<T>> GetAll();
    public Task<T> Add (T entity);
    public Task<T?> Update (T entity);
    public Task<bool> Delete (T entity);
}

