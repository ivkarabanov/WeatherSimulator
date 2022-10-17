using WeatherSimulator.Core.Models;

namespace WeatherSimulator.Core.Abstractions.Repositories;

public interface IRepository<T>
    where T: BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
        
    Task<T?> GetByIdAsync(Guid id);
    Task InsertAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}