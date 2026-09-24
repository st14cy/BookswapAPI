using BookswapAPI.Models.DTOs.Advertisement;

namespace BookswapAPI.Services.Favorites;

public interface IFavoriteService
{
    /// <summary>Id объявлений в избранном пользователя (чтобы подсветить «сердечки» в списках)</summary>
    Task<IReadOnlyList<Guid>> GetIdsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Избранные объявления пользователя (только опубликованные), новые сверху</summary>
    Task<IReadOnlyList<AdvertisementInfoDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Добавить в избранное. Повторное добавление ничего не меняет.</summary>
    /// <exception cref="KeyNotFoundException">объявление не найдено или снято с публикации</exception>
    Task AddAsync(Guid userId, Guid advertisementId, CancellationToken cancellationToken = default);

    /// <summary>Убрать из избранного (мягко: IsDeleted = true). Если не было в избранном — ничего не делает.</summary>
    Task RemoveAsync(Guid userId, Guid advertisementId, CancellationToken cancellationToken = default);
}
