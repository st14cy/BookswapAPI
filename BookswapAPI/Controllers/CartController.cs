using System.Security.Claims;
using BookswapAPI.Services.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookswapAPI.Controllers;

/// <summary>Корзина текущего пользователя. Все методы — только для авторизованных.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CartController(ICartService cartService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        return Ok(await cartService.GetAsync(userId.Value, cancellationToken));
    }

    [HttpGet("ids")]
    public async Task<IActionResult> GetIds(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        return Ok(await cartService.GetIdsAsync(userId.Value, cancellationToken));
    }

    [HttpPost("{advertisementId:guid}")]
    public async Task<IActionResult> Add(Guid advertisementId, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        try
        {
            await cartService.AddAsync(userId.Value, advertisementId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{advertisementId:guid}")]
    public async Task<IActionResult> Remove(Guid advertisementId, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        await cartService.RemoveAsync(userId.Value, advertisementId, cancellationToken);
        return NoContent();
    }

    /// <summary>«Оформить»: заказ из всех книг корзины</summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        try
        {
            return Ok(await cartService.CheckoutAsync(userId.Value, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid? GetUserIdFromClaims()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("userId")?.Value
                    ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
