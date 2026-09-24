using BookswapAPI.Data;
using BookswapAPI.Models;
using BookswapAPI.Models.DTOs.Advertisement;
using Microsoft.EntityFrameworkCore;

namespace BookswapAPI.Services.Favorites;

public class FavoriteService(AppDbContext context, ILogger<FavoriteService> logger) : IFavoriteService
{
    public async Task<IReadOnlyList<Guid>> GetIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.FavoriteAdvertisements
            .AsNoTracking()
            .Where(f => f.UserId == userId && !f.IsDeleted && !f.Advertisement.IsDeleted)
            .Select(f => f.AdvertisementId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdvertisementInfoDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.FavoriteAdvertisements
            .AsNoTracking()
            .Where(f => f.UserId == userId
                        && !f.IsDeleted
                        && !f.Advertisement.IsDeleted
                        && f.Advertisement.IsActive)
            .OrderByDescending(f => f.AddedAt)
            .Select(f => new AdvertisementInfoDto
            {
                Id = f.Advertisement.Id,
                Title = f.Advertisement.Title,
                Description = f.Advertisement.Description,
                AuthorName = f.Advertisement.Author,
                BookTitle = f.Advertisement.TitleBook,
                GenreId = f.Advertisement.GenreId,
                GenreName = f.Advertisement.Genre.Name,
                IsNew = f.Advertisement.IsNew,
                IsForever = f.Advertisement.IsForever,
                IsPostamat = f.Advertisement.IsPostamat,
                City = f.Advertisement.City,
                Street = f.Advertisement.Street,
                HouseNumber = f.Advertisement.HouseNumber,
                OwnerId = f.Advertisement.SellerId,
                OwnerName = f.Advertisement.Seller.Name,
                CreatedAt = f.Advertisement.CreatedAt,
                IsActive = f.Advertisement.IsActive,
                CoverUrl = f.Advertisement.CoverUrl,
                LikeCount = f.Advertisement.LikeCount
            })
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Guid userId, Guid advertisementId, CancellationToken cancellationToken = default)
    {
        var advertisement = await context.Advertisements
            .FirstOrDefaultAsync(a => a.Id == advertisementId && !a.IsDeleted && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Объявление не найдено");

        // Запись могла остаться после «удаления» из избранного (мягкое удаление) —
        // уникальный индекс (UserId, AdvertisementId) не даст создать вторую, поэтому восстанавливаем
        var favorite = await context.FavoriteAdvertisements
            .FirstOrDefaultAsync(f => f.UserId == userId && f.AdvertisementId == advertisementId, cancellationToken);

        var now = DateTime.UtcNow;
        if (favorite == null)
        {
            await context.FavoriteAdvertisements.AddAsync(new FavoriteAdvertisement
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AdvertisementId = advertisementId,
                AddedAt = now,
                CreatedAt = now,
                CreatedBy = userId,
                IsDeleted = false
            }, cancellationToken);
        }
        else if (favorite.IsDeleted)
        {
            favorite.IsDeleted = false;
            favorite.DeletedAt = null;
            favorite.DeletedBy = null;
            favorite.AddedAt = now;
            favorite.UpdatedAt = now;
            favorite.UpdatedBy = userId;
        }
        else
        {
            return; // уже в избранном
        }

        advertisement.LikeCount += 1;
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Пользователь {UserId} добавил объявление {AdId} в избранное", userId, advertisementId);
    }

    public async Task RemoveAsync(Guid userId, Guid advertisementId, CancellationToken cancellationToken = default)
    {
        var favorite = await context.FavoriteAdvertisements
            .Include(f => f.Advertisement)
            .FirstOrDefaultAsync(f => f.UserId == userId && f.AdvertisementId == advertisementId && !f.IsDeleted,
                cancellationToken);
        if (favorite == null)
            return; // и так не в избранном

        var now = DateTime.UtcNow;
        favorite.IsDeleted = true;
        favorite.DeletedAt = now;
        favorite.DeletedBy = userId;
        favorite.Advertisement.LikeCount = Math.Max(0, favorite.Advertisement.LikeCount - 1);

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Пользователь {UserId} убрал объявление {AdId} из избранного", userId, advertisementId);
    }
}
