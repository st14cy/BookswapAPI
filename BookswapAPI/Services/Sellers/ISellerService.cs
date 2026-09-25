using BookswapAPI.Models.DTOs.Sellers;

namespace BookswapAPI.Services.Sellers;

public interface ISellerService
{
    Task<SellerInfoDto> GetByIdAsync(Guid sellerId, CancellationToken cancellationToken = default);
}
