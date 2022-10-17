using WeatherSimulator.Core.Abstractions.Repositories;
using WeatherSimulator.Core.Models;

namespace WeatherSimulator.Client.Repositories;

public class InMemoryRepository<T>
        : IRepository<T>
        where T: BaseEntity
{
    private static IEnumerable<T> Data { get; set; } = new List<T>();
    private const int MaxDataCount = 1000 * 1000;
    private readonly object _locker = new object();

  public Task<IEnumerable<T>> GetAllAsync()
    {
        return Task.FromResult(Data);
    }

    public Task<T?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(Data.FirstOrDefault(x => x.Id == id));
    }

    public Task InsertAsync(T entity)
    {
        return Task.Run(() =>
        {
            lock (_locker)
            {
                var dataList = Data as List<T>;
                dataList?.Add(entity);
                if (dataList is { Count: > MaxDataCount })
                {
                    Data = dataList.OrderByDescending(x => x.LastUpdate).Take(MaxDataCount).OrderBy(x => x.LastUpdate)
                        .ToList();
                }
            }
        });
    }

    public Task UpdateAsync(T entity)
    {
        return Task.Run(() =>
        {
            
            var itemToUpdate = Data.FirstOrDefault(x => x.Id == entity.Id);
            foreach (var property in typeof(T).GetProperties().Where(x=>x.Name != nameof(BaseEntity.Id)))
            {
                if (property.SetMethod != null)
                {
                    var valueToUpdate = property.GetValue(entity);
                    property.SetValue(itemToUpdate, valueToUpdate);
                }
            }
        });
    }

    public Task DeleteAsync(Guid id)
    {
        return Task.Run(() =>
        {
            Data = Data.Where(x => x.Id != id).ToList();
        });
    }
}