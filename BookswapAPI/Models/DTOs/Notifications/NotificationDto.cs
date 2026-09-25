using BookswapAPI.Models.Enum;

namespace BookswapAPI.Models.DTOs.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? AdvertisementId { get; set; }
    public Guid? OrderId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
