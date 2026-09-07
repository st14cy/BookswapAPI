using BookswapAPI.Data;
using BookswapAPI.Models.DTOs.Advertisement;
using Microsoft.EntityFrameworkCore;

namespace BookswapAPI.Services.Advertisement;

public class AdvertisementService(AppDbContext context, ILogger<AdvertisementService> logger) : IAdvertisementService
{
    public async Task<IEnumerable<AdvertisementInfoDto>> GeAllAdvertisementAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await context.Advertisements
                .Include(x => x.Seller)
                .Include(x => x.Genre)
                .Where(x => !x.IsDeleted)
                .Select(x => new AdvertisementInfoDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    AuthorName = x.Author,
                    BookTitle = x.Title,
                    GenreId = x.GenreId,
                    GenreName = x.Genre.Name,
                    IsNew = x.IsNew,
                    IsForever = x.IsForever,
                    IsPostamat = x.IsPostamat,
                    City = x.City,
                    Street = x.Street,
                    HouseNumber = x.HouseNumber,
                    OwnerId = x.SellerId,
                    OwnerName = x.Seller.Name
                })
                .ToListAsync(cancellationToken);
            return res;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Не удалось выгрузить объявления");
            throw;
        }
    }

    public async Task<AdvertisementInfoDto> GetAdvertisementByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var advertisement = await context.Advertisements
                .Include(x => x.Seller)
                .Include(x => x.Genre)
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new AdvertisementInfoDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    BookTitle = x.Title,
                    AuthorName = x.Author,
                    GenreId = x.GenreId,
                    GenreName = x.Genre.Name,
                    IsNew = x.IsNew,
                    IsForever = x.IsForever,
                    IsPostamat = x.IsPostamat,
                    City = x.City,
                    Street = x.Street,
                    HouseNumber = x.HouseNumber,
                    OwnerId = x.SellerId,
                    OwnerName = x.Seller != null ? x.Seller.Name : "Неизвестно",
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (advertisement != null) return advertisement;
            
            logger.LogWarning("Объявление с ID {AdId} не найдено", id);
            throw new KeyNotFoundException($"Объявление с ID {id} не найдено");
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Запрос GetAdvertisementById для ID {AdId} был отменен", id);
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении объявления с ID {AdId}", id);
            throw;
        }
    }

    public async Task<AdvertisementInfoDto> CreateAdvertisementAsync(
        CreateAdvertisementRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var advertisement = new Models.Advertisement
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                Author = request.Author,
                GenreId = request.GenreId,
                IsNew = request.IsNew,
                IsForever = request.IsForever,
                IsPostamat = request.IsPostamat,
                City = request.City,
                Street = request.Street,
                HouseNumber = request.HouseNumber,
                SellerId = request.OwnerId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await context.Advertisements.AddAsync(advertisement, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            
            return await GetAdvertisementByIdAsync(advertisement.Id, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Создание объявления было отменено");
            throw;
        }
        catch (DbUpdateException e)
        {
            logger.LogError(e, "Ошибка при создании объявления");
            throw new InvalidOperationException("Не удалось создать объявление", e);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при создании объявления");
            throw;
        }
    }

    public async Task<AdvertisementInfoDto> UpdateAdvertisementAsync(
        Guid id,
        UpdateAdvertisementRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var advertisement = await context.Advertisements
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            if (advertisement == null)
            {
                logger.LogWarning("Объявление с ID {AdId} не найдено для обновления", id);
                throw new KeyNotFoundException($"Объявление с ID {id} не найдено");
            }

         
            if (!string.IsNullOrWhiteSpace(request.Title))
                advertisement.Title = request.Title;

            if (!string.IsNullOrWhiteSpace(request.Description))
                advertisement.Description = request.Description;

            if (!string.IsNullOrWhiteSpace(request.Author))
                advertisement.Author = request.Author;

            if (request.GenreId.HasValue)
                advertisement.GenreId = request.GenreId.Value;

            if (request.IsNew.HasValue)
                advertisement.IsNew = request.IsNew.Value;

            if (request.IsForever.HasValue)
                advertisement.IsForever = request.IsForever.Value;

            if (request.IsPostamat.HasValue)
                advertisement.IsPostamat = request.IsPostamat.Value;

            if (!string.IsNullOrWhiteSpace(request.City))
                advertisement.City = request.City;

            if (!string.IsNullOrWhiteSpace(request.Street))
                advertisement.Street = request.Street;

            if (!string.IsNullOrWhiteSpace(request.HouseNumber))
                advertisement.HouseNumber = request.HouseNumber;

            advertisement.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return await GetAdvertisementByIdAsync(id, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Обновление объявления с ID {AdId} было отменено", id);
            throw;
        }
        catch (DbUpdateException e)
        {
            logger.LogError(e, "Ошибка при обновлении объявления с ID {AdId}", id);
            throw new InvalidOperationException($"Не удалось обновить объявление с ID {id}", e);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при обновлении объявления с ID {AdId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAdvertisementAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var advertisement = await context.Advertisements
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            if (advertisement == null)
            {
                logger.LogWarning("Объявление с ID {AdId} не найдено для удаления", id);
                return false;
            }

            // Soft delete - помечаем как удаленное
            advertisement.IsDeleted = true;
            advertisement.DeletedAt = DateTime.UtcNow;
            advertisement.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Объявление с ID {AdId} успешно удалено", id);
            return true;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Удаление объявления с ID {AdId} было отменено", id);
            throw;
        }
        catch (DbUpdateException e)
        {
            logger.LogError(e, "Ошибка при удалении объявления с ID {AdId}", id);
            throw new InvalidOperationException($"Не удалось удалить объявление с ID {id}", e);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при удалении объявления с ID {AdId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<AdvertisementInfoDto>> GetAdvertisementByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var advertisements = await context.Advertisements
                .Include(x => x.Seller)
                .Include(x => x.Genre)
                .Where(x => x.SellerId == userId && !x.IsDeleted)
                .Select(x => new AdvertisementInfoDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    BookTitle = x.Title,
                    AuthorName = x.Author,
                    GenreId = x.GenreId,
                    GenreName = x.Genre.Name,
                    IsNew = x.IsNew,
                    IsForever = x.IsForever,
                    IsPostamat = x.IsPostamat,
                    City = x.City,
                    Street = x.Street,
                    HouseNumber = x.HouseNumber,
                    OwnerId = x.SellerId,
                    OwnerName = x.Seller.Name,
                    CreatedAt = x.CreatedAt
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return advertisements;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Запрос объявлений пользователя {UserId} был отменен", userId);
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при получении объявлений пользователя {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<AdvertisementInfoDto>> SearchAdvertisementAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GeAllAdvertisementAsync(cancellationToken);
            
            var searchTermLower = searchTerm.ToLowerInvariant();

            var advertisements = await context.Advertisements
                .Include(x => x.Seller)
                .Include(x => x.Genre)
                .Where(x => !x.IsDeleted &&
                    (x.Title.ToLower().Contains(searchTermLower) ||
                     x.Description.ToLower().Contains(searchTermLower) ||
                     x.Author.ToLower().Contains(searchTermLower) ||
                     x.Genre.Name.ToLower().Contains(searchTermLower) ||
                     x.City.ToLower().Contains(searchTermLower)))
                .Select(x => new AdvertisementInfoDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    BookTitle = x.Title,
                    AuthorName = x.Author,
                    GenreId = x.GenreId,
                    GenreName = x.Genre.Name,
                    IsNew = x.IsNew,
                    IsForever = x.IsForever,
                    IsPostamat = x.IsPostamat,
                    City = x.City,
                    Street = x.Street,
                    HouseNumber = x.HouseNumber,
                    OwnerId = x.SellerId,
                    OwnerName = x.Seller.Name,
                    CreatedAt = x.CreatedAt
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return advertisements;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Поиск объявлений по запросу '{SearchTerm}' был отменен", searchTerm);
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Ошибка при поиске объявлений по запросу '{SearchTerm}'", searchTerm);
            throw;
        }
    }
}