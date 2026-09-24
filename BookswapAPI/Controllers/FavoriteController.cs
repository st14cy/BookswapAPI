using System.Security.Claims;
using BookswapAPI.Services.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookswapAPI.Controllers;

/// <summary>
/// Избранные объявления текущего пользователя. Все методы — только для авторизованных.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FavoriteController(IFavoriteService favoriteService) : ControllerBase
{
    /// <summary>Список избранных объявлений</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        return Ok(await favoriteService.GetAllAsync(userId.Value, cancellationToken));
    }

    /// <summary>Только Id избранных объявлений — для «сердечек» в списках</summary>
    [HttpGet("ids")]
    public async Task<IActionResult> GetIds(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        return Ok(await favoriteService.GetIdsAsync(userId.Value, cancellationToken));
    }

    /// <summary>Добавить объявление в избранное</summary>
    [HttpPost("{advertisementId:guid}")]
    public async Task<IActionResult> Add(Guid advertisementId, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        try
        {
            await favoriteService.AddAsync(userId.Value, advertisementId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Убрать объявление из избранного</summary>
    [HttpDelete("{advertisementId:guid}")]
    public async Task<IActionResult> Remove(Guid advertisementId, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        await favoriteService.RemoveAsync(userId.Value, advertisementId, cancellationToken);
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
