namespace BookswapAPI.Models.DTOs.Sellers;

public class SellerInfoDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string? Image { get; set; }
    public DateTime RegisteredAt { get; set; }
    public int AdvertisementsCount { get; set; }
}
