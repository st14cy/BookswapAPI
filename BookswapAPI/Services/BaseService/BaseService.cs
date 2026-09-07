using BookswapAPI.Data;
using BookswapAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookswapAPI.Services.BaseService;

  public class BaseService<T> : IBaseService<T> where T : BaseEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger<BaseService<T>> _logger;

        public BaseService(AppDbContext appDbContext, 
            ILogger<BaseService<T>> logger)
        {
            _context = appDbContext;
            _dbSet = _context.Set<T>();
            _logger = logger;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка GET по ID запросe с ${id}\r{ex.Message}");
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _dbSet.Where(x => !x.IsDeleted).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Не удалось вывести все записи(без удаленных)\r{ex.Message}");
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> GetWithDeleteAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Не удалось вывести все записи(вместе с удаленными)\r{ex.Message}");
                throw;
            }
        }

        public async Task<bool> Del(Guid id)
        {
            try
            {
                var entity = await _dbSet.FirstOrDefaultAsync(x => x.Id == id);
                if (entity == null)
                {
                    _logger.LogWarning($"Сущность с ID {id} не найдена для мягкого удаления");
                    return false;
                }

                if (entity.IsDeleted)
                {
                    _logger.LogWarning($"Сущность с ID {id} уже удалена");
                    return false;
                }

                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Сущность с ID {id} мягко удалена");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при мягком удалении сущности с ID {id}\r{ex.Message}");
                throw;
            }
        }

        public async Task<T> AddAsync(T entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.IsDeleted = false;
               
                await _dbSet.AddAsync(entity);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Сущность типа {typeof(T).Name} успешно добавлена с ID {entity.Id}");

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при добавлении сущности типа {typeof(T).Name}\r{ex.Message}");
                throw;
            }
        }
    }