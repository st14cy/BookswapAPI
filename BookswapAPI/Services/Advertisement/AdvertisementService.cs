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
                .Where(x => !x.IsDeleted && x.IsActive) // в каталоге только опубликованные
                .Select(x => new AdvertisementInfoDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    AuthorName = x.Author,
                    BookTitle = x.TitleBook,
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
                    IsActive = x.IsActive,
                    CoverUrl = x.CoverUrl,
                    LikeCount = x.LikeCount
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
                    BookTitle = x.TitleBook,
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
                    IsActive = x.IsActive,
                    CoverUrl = x.CoverUrl,
                    LikeCount = x.LikeCount,
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
        Guid userId,
        CreateAdvertisementRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var genreExists = await context.Genres
                .AnyAsync(g => g.Id == request.GenreId && !g.IsDeleted, cancellationToken);
            if (!genreExists)
                throw new ArgumentException("Выберите жанр из списка");

            var seller = await GetOrCreateSellerAsync(userId, cancellationToken);

            var advertisement = new Models.Advertisement
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                TitleBook= request.BookTitle,
                Author = request.AuthorName,
                GenreId = request.GenreId,
                IsNew = request.IsNew,
                IsForever = request.IsForever,
                IsPostamat = request.IsPostamat,
                City = request.City,
                Street = request.Street,
                HouseNumber = request.HouseNumber,
                SellerId = seller.Id,
                IsActive = true,           // новое объявление сразу опубликовано
                CoverUrl = NormalizeCoverUrl(request.CoverUrl),
                StartDate = DateTime.UtcNow,
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

    public async Task<AdvertisementInfoDto> SetActiveAsync(
        Guid userId,
        Guid advertisementId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var advertisement = await context.Advertisements
            .Include(x => x.Seller)
            .FirstOrDefaultAsync(x => x.Id == advertisementId && !x.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException("Объявление не найдено");

        // Снять с публикации / вернуть может только владелец
        if (advertisement.Seller.UserId != userId)
            throw new UnauthorizedAccessException("Это не ваше объявление");

        if (advertisement.IsActive != isActive)
        {
            advertisement.IsActive = isActive;
            advertisement.UpdatedAt = DateTime.UtcNow;
            advertisement.UpdatedBy = userId;
            if (isActive)
                advertisement.StartDate = DateTime.UtcNow;
            else
                advertisement.EndDate = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Объявление {AdId} {Action}", advertisementId,
                isActive ? "опубликовано снова" : "снято с публикации");
        }

        return await GetAdvertisementByIdAsync(advertisementId, cancellationToken);
    }

    /// <summary>
    /// Обложки берём только с OpenLibrary — произвольные ссылки не сохраняем.
    /// </summary>
    private static string? NormalizeCoverUrl(string? coverUrl)
    {
        if (string.IsNullOrWhiteSpace(coverUrl))
            return null;

        var url = coverUrl.Trim();
        if (url.Length > 500 || !url.StartsWith("https://covers.openlibrary.org/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Недопустимая ссылка на обложку");

        return url;
    }

    /// <summary>
    /// Объявления привязаны к продавцу (Seller), а не напрямую к пользователю.
    /// Если у пользователя ещё нет профиля продавца (не указал имя при регистрации) — создаём его.
    /// </summary>
    private async Task<Models.Seller> GetOrCreateSellerAsync(Guid userId, CancellationToken cancellationToken)
    {
        var seller = await context.Sellers
            .FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted, cancellationToken);
        if (seller != null)
            return seller;

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken)
            ?? throw new UnauthorizedAccessException("Пользователь не найден. Войдите в аккаунт заново");

        seller = new Models.Seller
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = user.Login,
            Rating = 0.0,
            Image = "default.jpg",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        await context.Sellers.AddAsync(seller, cancellationToken);
        return seller;
    }

    public async Task<AdvertisementInfoDto> UpdateAdvertisementAsync(
        Guid userId,
        Guid id,
        UpdateAdvertisementRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var advertisement = await context.Advertisements
                .Include(x => x.Seller)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            if (advertisement == null)
            {
                logger.LogWarning("Объявление с ID {AdId} не найдено для обновления", id);
                throw new KeyNotFoundException($"Объявление с ID {id} не найдено");
            }

            // Редактировать может только владелец
            if (advertisement.Seller.UserId != userId)
                throw new UnauthorizedAccessException("Это не ваше объявление");

            if (request.GenreId.HasValue && request.GenreId.Value != advertisement.GenreId)
            {
                var genreExists = await context.Genres
                    .AnyAsync(g => g.Id == request.GenreId.Value && !g.IsDeleted, cancellationToken);
                if (!genreExists)
                    throw new ArgumentException("Выберите жанр из списка");
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
                advertisement.Title = request.Title;

            if (!string.IsNullOrWhiteSpace(request.BookTitle))
                advertisement.TitleBook = request.BookTitle;

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

            // null — обложку не трогаем, пустая строка — убираем
            if (request.CoverUrl != null)
                advertisement.CoverUrl = NormalizeCoverUrl(request.CoverUrl);

            advertisement.UpdatedAt = DateTime.UtcNow;
            advertisement.UpdatedBy = userId;

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
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var advertisement = await context.Advertisements
                .Include(x => x.Seller)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

            if (advertisement == null)
            {
                logger.LogWarning("Объявление с ID {AdId} не найдено для удаления", id);
                return false;
            }

            // Удалить может только владелец
            if (advertisement.Seller.UserId != userId)
                throw new UnauthorizedAccessException("Это не ваше объявление");

            // Soft delete - помечаем как удаленное
            advertisement.IsDeleted = true;
            advertisement.DeletedAt = DateTime.UtcNow;
            advertisement.DeletedBy = userId;
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
                // userId — это Id пользователя (User), объявления привязаны к его продавцу (Seller)
                .Where(x => x.Seller.UserId == userId && !x.IsDeleted)
                .Select(x => new AdvertisementInfoDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    BookTitle = x.TitleBook,
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
                    IsActive = x.IsActive,
                    CoverUrl = x.CoverUrl,
                    LikeCount = x.LikeCount,
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
            
            var searchTermLower = searchTerm.Trim().ToLower();

            var advertisements = await context.Advertisements
                .Include(x => x.Seller)
                .Include(x => x.Genre)
                .Where(x => !x.IsDeleted && x.IsActive &&
                    (x.TitleBook.ToLower().Contains(searchTermLower) ||
                     x.Title.ToLower().Contains(searchTermLower) ||
                     x.Description.ToLower().Contains(searchTermLower) ||
                     x.Author.ToLower().Contains(searchTermLower) ||
                     x.Genre.Name.ToLower().Contains(searchTermLower) ||
                     x.City.ToLower().Contains(searchTermLower)))
                .Select(x => new AdvertisementInfoDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    BookTitle = x.TitleBook,
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
                    IsActive = x.IsActive,
                    CoverUrl = x.CoverUrl,
                    LikeCount = x.LikeCount,
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