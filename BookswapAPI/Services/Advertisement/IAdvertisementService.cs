using BookswapAPI.Models.DTOs.Advertisement;
using BookswapAPI.Services.Auth;

namespace BookswapAPI.Services.Advertisement;

public interface IAdvertisementService
{
    Task<IEnumerable<AdvertisementInfoDto>> GeAllAdvertisementAsync( CancellationToken cancellationToken = default);
    
    Task<AdvertisementInfoDto> GetAdvertisementByIdAsync(Guid id,  CancellationToken cancellationToken = default);
    /// <param name="userId">Id авторизованного пользователя (из JWT) — он становится продавцом</param>
    Task<AdvertisementInfoDto> CreateAdvertisementAsync(Guid userId, CreateAdvertisementRequestDto request,  CancellationToken cancellationToken = default);
    /// <param name="userId">Id авторизованного пользователя — редактировать может только владелец</param>
    /// <exception cref="KeyNotFoundException">объявление не найдено</exception>
    /// <exception cref="UnauthorizedAccessException">объявление принадлежит другому пользователю</exception>
    Task<AdvertisementInfoDto> UpdateAdvertisementAsync(Guid userId, Guid id, UpdateAdvertisementRequestDto request,  CancellationToken cancellationToken = default);
    /// <summary>
    /// Мягкое удаление (IsDeleted = true). Только для владельца.
    /// </summary>
    /// <returns>false — объявление не найдено</returns>
    /// <exception cref="UnauthorizedAccessException">объявление принадлежит другому пользователю</exception>
    Task<bool> DeleteAdvertisementAsync(Guid userId, Guid id,  CancellationToken cancellationToken = default);
    /// <param name="userId">Id пользователя (User), а не продавца</param>
    Task<IEnumerable<AdvertisementInfoDto>> GetAdvertisementByUserAsync(Guid userId,  CancellationToken cancellationToken = default);
    /// <summary>
    /// Снять с публикации (isActive = false, объявление уходит в архив) или опубликовать снова.
    /// Только для владельца объявления.
    /// </summary>
    /// <exception cref="KeyNotFoundException">объявление не найдено</exception>
    /// <exception cref="UnauthorizedAccessException">объявление принадлежит другому пользователю</exception>
    Task<AdvertisementInfoDto> SetActiveAsync(Guid userId, Guid advertisementId, bool isActive, CancellationToken cancellationToken = default);
    Task<IEnumerable<AdvertisementInfoDto>> SearchAdvertisementAsync(string searchTerm,  CancellationToken cancellationToken = default);
}