using System.Linq.Expressions;

namespace UserManagement.Infrastructure.Interfaces;

public interface IRepository<T>
{
    Task AddAsync(T entity);
    Task<T?> GetByIDAsync(int id);
    Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> GetAllAsync();
    Task<bool> UpdateAsync(T entity);
}
