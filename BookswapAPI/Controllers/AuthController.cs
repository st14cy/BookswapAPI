using BookswapAPI.Models.DTOs;
using BookswapAPI.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookswapAPI.Controllers;

/// <summary>
/// Контроллер для аутентификации и авторизации пользователей
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <param name="dto">Данные для регистрации</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Токены доступа и информация о пользователе</returns>
    /// <response code="200">Успешная регистрация</response>
    /// <response code="400">Ошибка валидации или пользователь уже существует</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto, cancellationToken);
            _logger.LogInformation($"Успешная регистрация пользователя {dto.Login}");
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Ошибка регистрации: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Регистрация была отменена клиентом");
            return StatusCode(499, new { message = "Запрос был отменен" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка при регистрации пользователя {Login}", dto.Login);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера. Попробуйте позже." });
        }
    }

    /// <summary>
    /// Вход в систему
    /// </summary>
    /// <param name="dto">Данные для входа (логин и пароль)</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Токены доступа и информация о пользователе</returns>
    /// <response code="200">Успешный вход</response>
    /// <response code="401">Неверный логин или пароль</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.LoginAsync(dto, cancellationToken);
            _logger.LogInformation($"Успешный вход пользователя {dto.Login}");
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Неудачная попытка входа для пользователя {dto.Login}: {ex.Message}");
            return Unauthorized(new { message = ex.Message });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Вход был отменен клиентом");
            return StatusCode(499, new { message = "Запрос был отменен" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка при входе пользователя {Login}", dto.Login);
            return StatusCode(500, new { message = "Внутренняя ошибка сервера. Попробуйте позже." });
        }
    }

    /// <summary>
    /// Обновление JWT токена с помощью Refresh Token
    /// </summary>
    /// <param name="dto">Refresh токен</param>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Новая пара токенов</returns>
    /// <response code="200">Токен успешно обновлен</response>
    /// <response code="401">Неверный или истекший refresh токен</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken, cancellationToken);
            _logger.LogInformation("Refresh токен успешно обновлен");
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning($"Неудачная попытка обновления токена: {ex.Message}");
            return Unauthorized(new { message = ex.Message });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Обновление токена было отменено клиентом");
            return StatusCode(499, new { message = "Запрос был отменен" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка при обновлении токена");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера. Попробуйте позже." });
        }
    }

    /// <summary>
    /// Выход из системы (удаление refresh токена)
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Результат операции</returns>
    /// <response code="200">Успешный выход</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized(new { message = "Пользователь не авторизован" });

            var result = await _authService.LogoutAsync(userId.Value, cancellationToken);
            
            if (!result)
                return NotFound(new { message = "Пользователь не найден" });

            _logger.LogInformation($"Пользователь {userId} успешно вышел из системы");
            return Ok(new { message = "Выход выполнен успешно" });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Выход был отменен клиентом");
            return StatusCode(499, new { message = "Запрос был отменен" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка при выходе из системы");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера. Попробуйте позже." });
        }
    }

    /// <summary>
    /// Получение информации о текущем авторизованном пользователе
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции</param>
    /// <returns>Информация о пользователе</returns>
    /// <response code="200">Информация получена успешно</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Пользователь не найден</response>
    /// <response code="500">Внутренняя ошибка сервера</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized(new { message = "Пользователь не авторизован" });

            var userInfo = await _authService.GetUserInfoAsync(userId.Value, cancellationToken);
            return Ok(userInfo);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning($"Пользователь не найден: {ex.Message}");
            return NotFound(new { message = "Пользователь не найден" });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Запрос информации о пользователе был отменен");
            return StatusCode(499, new { message = "Запрос был отменен" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка при получении информации о пользователе");
            return StatusCode(500, new { message = "Внутренняя ошибка сервера. Попробуйте позже." });
        }
    }

    /// <summary>
    /// Проверка доступности API (без авторизации)
    /// </summary>
    /// <returns>Статус сервера</returns>
    /// <response code="200">Сервер работает</response>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            status = "OK", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
    

    /// <summary>
    /// Получение ID пользователя из claims
    /// </summary>
    private Guid? GetUserIdFromClaims()
    {
        // Пробуем получить ID из разных источников
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("userId")?.Value
                          ?? User.FindFirst("sub")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim))
            return null;

        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }

    /// <summary>
    /// Проверка, является ли пользователь администратором
    /// </summary>
    private bool IsUserInRole(string role)
    {
        return User.IsInRole(role);
    }
}