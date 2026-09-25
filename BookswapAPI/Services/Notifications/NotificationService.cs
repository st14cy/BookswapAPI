using BookswapAPI.Data;
using BookswapAPI.Models.DTOs.Notifications;
using Microsoft.EntityFrameworkCore;

namespace BookswapAPI.Services.Notifications;

public class NotificationService(AppDbContext context) : INotificationService
{
    public async Task<IReadOnlyList<NotificationDto>> GetAsync(Guid userId, int take = 50, CancellationToken cancellationToken = default)
    {
        return await context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .Take(Math.Clamp(take, 1, 200))
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                AdvertisementId = n.AdvertisementId,
                OrderId = n.OrderId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsDeleted && !n.IsRead, cancellationToken);
    }

    public async Task MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted, cancellationToken)
            ?? throw new KeyNotFoundException("Уведомление не найдено");

        if (notification.IsRead) return;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await context.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted && !n.IsRead)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now), cancellationToken);
    }
}
