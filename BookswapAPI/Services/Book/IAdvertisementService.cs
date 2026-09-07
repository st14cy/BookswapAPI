using BookswapAPI.Models.DTOs.Advertisement;
using BookswapAPI.Services.Auth;

namespace BookswapAPI.Services.Book;

public interface IAdvertisementService
{
    Task<IEnumerable<AdvertisementInfoDto>> GeAllAdvertisementAsync(  CancellationToken cancellationToken = default);
    
    Task<AdvertisementInfoDto> GetAdvertisementByIdAsync(Guid id,  CancellationToken cancellationToken = default);
    Task<AdvertisementInfoDto> CreateAdvertisementAsync(CreateAdvertisementRequestDto request,  CancellationToken cancellationToken = default);
    Task<AdvertisementInfoDto> UpdateAdvertisementAsync(Guid id, UpdateAdvertisementRequestDto request,  CancellationToken cancellationToken = default);
    Task<bool> DeleteAdvertisementAsync(Guid id,  CancellationToken cancellationToken = default);
    Task<IEnumerable<AdvertisementInfoDto>> GetAdvertisementByUserAsync(Guid userId,  CancellationToken cancellationToken = default);
    Task<IEnumerable<AdvertisementInfoDto>> SearchAdvertisementAsync(string searchTerm,  CancellationToken cancellationToken = default);
}