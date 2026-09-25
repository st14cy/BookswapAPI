using System.ComponentModel.DataAnnotations;
using BookswapAPI.Models.Enum;

namespace BookswapAPI.Models;

/// <summary>Уведомление пользователю (например, продавцу — что его книгу забрали)</summary>
public class Notification : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public NotificationType Type { get; set; }

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public Guid? AdvertisementId { get; set; }
    public Guid? OrderId { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
