using BookswapAPI.Models;
using BookswapAPI.Models.DTOs.Advertisement;
using System.Security.Claims;
using BookswapAPI.Services.Advertisement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookswapAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdvertisementController : ControllerBase
{
    public AdvertisementController(IAdvertisementService advertisementService)
    {
        _advertisementService = advertisementService;
    }
    
    private readonly IAdvertisementService  _advertisementService;

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var advertisement=_advertisementService.GeAllAdvertisementAsync();
        return Ok(await advertisement);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query, CancellationToken cancellationToken)
    {
        var advertisements = await _advertisementService.SearchAdvertisementAsync(query ?? string.Empty, cancellationToken);
        return Ok(advertisements);
    }

    [HttpGet("getById/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var get = _advertisementService.GetAdvertisementByIdAsync(id, cancellationToken);
        return Ok(await get);
    }
    
    /// <summary>
    /// Объявления текущего авторизованного пользователя (раздел «Мои объявления»)
    /// </summary>
    [Authorize]
    [HttpGet("my")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        var posts = await _advertisementService.GetAdvertisementByUserAsync(userId.Value, cancellationToken);
        return Ok(posts);
    }

    /// <summary>
    /// Снять объявление с публикации — оно уходит в архив (IsActive = false)
    /// </summary>
    [Authorize]
    [HttpPatch("{id:guid}/unpublish")]
    public Task<IActionResult> Unpublish(Guid id, CancellationToken cancellationToken)
        => SetActive(id, false, cancellationToken);

    /// <summary>
    /// Вернуть объявление из архива в публикацию (IsActive = true)
    /// </summary>
    [Authorize]
    [HttpPatch("{id:guid}/publish")]
    public Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
        => SetActive(id, true, cancellationToken);

    private async Task<IActionResult> SetActive(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        try
        {
            var post = await _advertisementService.SetActiveAsync(userId.Value, id, isActive, cancellationToken);
            return Ok(post);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Удаление объявления (мягкое: IsDeleted = true). Только для владельца.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        try
        {
            var deleted = await _advertisementService.DeleteAdvertisementAsync(userId.Value, id, cancellationToken);
            if (!deleted)
                return NotFound(new { message = "Объявление не найдено" });

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Объявления пользователя по его Id (User.Id)
    /// </summary>
    [HttpGet("getByUser/{id:guid}")]
    public async Task<IActionResult> GetByUserId(Guid id, CancellationToken cancellationToken)
    {
        var get = _advertisementService.GetAdvertisementByUserAsync(id, cancellationToken);
        return Ok(await get);
    }
    
    
    
    /// <summary>
    /// Создание объявления. Владелец — текущий авторизованный пользователь (из JWT).
    /// </summary>
    [Authorize]
    [HttpPost("createAdvertisement")]
    public async Task<IActionResult> CreateAdvertisement([FromBody] CreateAdvertisementRequestDto advertisement,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        try
        {
            var post = await _advertisementService.CreateAdvertisementAsync(userId.Value, advertisement, cancellationToken);
            return Ok(post);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
    
    
    /// <summary>
    /// Редактирование объявления. Только для владельца.
    /// </summary>
    [Authorize]
    [HttpPut("posts/{id:guid}")]
    public async Task<IActionResult> UpdateAdvertisement(
        Guid id,
        [FromBody] UpdateAdvertisementRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (dto is null)
            return BadRequest(new { message = "Тело запроса пустое." });

        var userId = GetUserIdFromClaims();
        if (userId == null)
            return Unauthorized(new { message = "Необходимо войти в аккаунт" });

        try
        {
            var post = await _advertisementService
                .UpdateAdvertisementAsync(userId.Value, id, dto, cancellationToken);
            return Ok(post);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Объявление не найдено" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Id пользователя из JWT (так же, как в AuthController)
    /// </summary>
    private Guid? GetUserIdFromClaims()
    {
        var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("userId")?.Value
                    ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
