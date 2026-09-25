using System.Security.Claims;
using BookswapAPI.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookswapAPI.Controllers;

/// <summary>Уведомления текущего пользователя. Все методы — только для авторизованных.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationController(INotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        return Ok(await notificationService.GetAsync(userId.Value, cancellationToken: cancellationToken));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        return Ok(new { count = await notificationService.GetUnreadCountAsync(userId.Value, cancellationToken) });
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        try
        {
            await notificationService.MarkReadAsync(userId.Value, id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        await notificationService.MarkAllReadAsync(userId.Value, cancellationToken);
        return NoContent();
    }

    private Guid? GetUserIdFromClaims()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("userId")?.Value
                    ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
