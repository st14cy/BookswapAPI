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

    [HttpGet("getById/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var get = _advertisementService.GetAdvertisementByIdAsync(id, cancellationToken);
        return Ok(await get);
    }
    
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
    
    
    [HttpPut("posts/{id:guid}")]
    public async Task<IActionResult> UpdateAdvertisement(
        Guid id,
        [FromBody] UpdateAdvertisementRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (dto is null)
            return BadRequest("Тело запроса пустое.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var post = await _advertisementService
                .UpdateAdvertisementAsync(id, dto, cancellationToken);

            if (post is null)
                return NotFound($"Объявление {id} не найдено.");

            return Ok(post);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Внутренняя ошибка сервера");
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
