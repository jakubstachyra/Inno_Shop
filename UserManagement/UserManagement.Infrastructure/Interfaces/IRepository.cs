namespace UserManagement.Infrastructure.Interfaces;

public interface IRepository<T>
{
    Task AddAsync(T entity);
    Task SaveChangesAsync();
}
