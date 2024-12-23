using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using UserManagement.Infrastructure.Interfaces;

namespace UserManagement.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;
    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIDAsync(int ID)  
    {
       return  await _dbSet.FindAsync(ID);
    }
    public async Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }
    public async Task AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }
}
