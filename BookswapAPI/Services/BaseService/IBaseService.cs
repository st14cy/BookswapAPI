using BookswapAPI.Models;

namespace BookswapAPI.Services.BaseService;

public interface IBaseService<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();

    Task<IEnumerable<T>> GetWithDeleteAsync();

    Task<bool> Del(Guid id);
    Task<T> AddAsync(T entity);
}

