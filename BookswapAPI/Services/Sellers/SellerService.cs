using BookswapAPI.Data;
using BookswapAPI.Models.DTOs.Sellers;
using Microsoft.EntityFrameworkCore;

namespace BookswapAPI.Services.Sellers;

public class SellerService(AppDbContext context) : ISellerService
{
    public async Task<SellerInfoDto> GetByIdAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var seller = await context.Sellers
            .AsNoTracking()
            .Where(s => s.Id == sellerId && !s.IsDeleted)
            .Select(s => new SellerInfoDto
            {
                Id = s.Id,
                Name = s.Name,
                Rating = s.Rating,
                Image = s.Image,
                RegisteredAt = s.User.CreatedAt,
                AdvertisementsCount = s.Advertisements.Count(a => !a.IsDeleted && a.IsActive)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return seller ?? throw new KeyNotFoundException("Продавец не найден");
    }
}
