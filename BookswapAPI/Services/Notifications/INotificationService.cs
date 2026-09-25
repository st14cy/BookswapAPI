using BookswapAPI.Models.DTOs.Notifications;

namespace BookswapAPI.Services.Notifications;

public interface INotificationService
{
    /// <summary>Последние уведомления пользователя, новые сверху</summary>
    Task<IReadOnlyList<NotificationDto>> GetAsync(Guid userId, int take = 50, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);

    Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
