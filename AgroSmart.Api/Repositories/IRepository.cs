using System.Linq.Expressions;

namespace AgroSmart.Api.Repositories;

/// <summary>Generic data-access abstraction shared by all entity repositories.</summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync();

    /// <summary>Composable query root for advanced read scenarios (includes, filtering, projections).</summary>
    IQueryable<T> Query();
}
