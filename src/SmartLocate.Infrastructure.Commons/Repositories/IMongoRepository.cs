using System.Linq.Expressions;

namespace SmartLocate.Infrastructure.Commons.Repositories;

public interface IMongoRepository<T> where T : class
{
    Task CreateAsync(T entity);
    
    Task<T> GetAsync(Guid id);
    
    Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter);
    
    Task<List<T>> GetAllAsync();
    
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter);
    
    Task<List<T>> GetAllAsync(int skip, int take, Expression<Func<T, object>> orderBy, bool orderByDescending = false);
    
    Task<List<T>> GetAllAsync(Expression<Func<T, bool>> filter, int skip, int take, Expression<Func<T, object>> orderBy, bool orderByDescending = false);
    
    Task UpdateAsync(T entity);
    
    Task RemoveAsync(Guid id);
    
    Task<long> CountAsync();
    
    Task<long> CountAsync(Expression<Func<T, bool>> filter);

    Task<bool> AnyAsync();
    
    Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
}