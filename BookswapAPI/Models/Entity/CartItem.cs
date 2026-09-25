using System.ComponentModel.DataAnnotations;

namespace BookswapAPI.Models;

/// <summary>Книга (объявление) в корзине пользователя</summary>
public class CartItem : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required]
    public Guid AdvertisementId { get; set; }
    public Advertisement Advertisement { get; set; } = null!;

    public DateTime AddedAt { get; set; }
}
