using BookswapAPI.Models.DTOs.Advertisement;

namespace BookswapAPI.Models.DTOs.Cart;

public class CartItemDto
{
    public DateTime AddedAt { get; set; }
    public AdvertisementInfoDto Advertisement { get; set; } = null!;
}
