using System.ComponentModel.DataAnnotations;

namespace BookswapAPI.Models;

/// <summary>Книга в заказе. Название и автор копируются на момент оформления.</summary>
public class OrderItem : BaseEntity
{
    [Required]
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [Required]
    public Guid AdvertisementId { get; set; }
    public Advertisement Advertisement { get; set; } = null!;

    [Required]
    public Guid SellerId { get; set; }

    [Required]
    [MaxLength(300)]
    public string BookTitle { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Author { get; set; } = string.Empty;
}
