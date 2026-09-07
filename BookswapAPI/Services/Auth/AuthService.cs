using BookswapAPI.Data;
using BookswapAPI.Models;
using BookswapAPI.Models.DTOs;
using BookswapAPI.Models.Enum;
using BookswapAPI.Services.JWT;
using Microsoft.EntityFrameworkCore;

namespace BookswapAPI.Services.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _config;

    public AuthService(
        AppDbContext context,
        IJwtService jwtService,
        ILogger<AuthService> logger,
        IConfiguration config)
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterDto dto, 
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Login == dto.Login || u.Email == dto.Email, 
                cancellationToken); 

        if (existingUser != null)
        {
            var duplicateField = existingUser.Login == dto.Login ? "логином" : "email";
            var duplicateValue = existingUser.Login == dto.Login ? dto.Login : dto.Email;
            throw new InvalidOperationException($"Пользователь с {duplicateField} '{duplicateValue}' уже существует");
        }
        
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = dto.Login,
            Email = dto.Email,
            PasswordHash = passwordHash,
            Role = Roles.user,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        
        if (!string.IsNullOrEmpty(dto.FirstName))
        {
            var seller = new Seller
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = dto.FirstName,
                Surname = dto.LastName ?? string.Empty,
                Rating = 0.0,
                Image = "default.jpg",
                CreatedAt = user.CreatedAt,
                IsDeleted = false
            };
            seller.User = user;
            await _context.Sellers.AddAsync(seller, cancellationToken); 
        }

        await _context.Users.AddAsync(user, cancellationToken);  
        await _context.SaveChangesAsync(cancellationToken); 

        _logger.LogInformation($"Пользователь {user.Login} успешно зарегистрирован с ID {user.Id}");
        
        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginDto dto, 
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Login == dto.Login && !u.IsDeleted, 
                cancellationToken); 

        if (user == null)
        {
            _logger.LogWarning($"Попытка входа не удалась: пользователь '{dto.Login}' не найден");
            throw new UnauthorizedAccessException("Неверный логин или пароль");
        }
        
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning($"Попытка входа не удалась: неверный пароль для пользователя '{dto.Login}'");
            throw new UnauthorizedAccessException("Неверный логин или пароль");
        }
        
        if (user.IsDeleted)
        {
            _logger.LogWarning($"Попытка входа не удалась: пользователь '{dto.Login}' удален");
            throw new UnauthorizedAccessException("Аккаунт удален. Обратитесь к администратору");
        }
        
        await _context.SaveChangesAsync(cancellationToken); 
        _logger.LogInformation($"Пользователь {user.Login} успешно вошел в систему");
        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            _logger.LogWarning("Попытка обновления токена не удалась: токен не предоставлен");
            throw new UnauthorizedAccessException("Refresh токен не предоставлен");
        }
        
        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.RefreshToken == refreshToken && !u.IsDeleted, 
                cancellationToken);

        if (user == null)
        {
            _logger.LogWarning($"Попытка обновления токена не удалась: токен не найден");
            throw new UnauthorizedAccessException("Неверный или истекший refresh токен");
        }
        
        var (isValid, _) = _jwtService.ValidateRefreshToken(user, refreshToken);
        if (!isValid)
        {
            _logger.LogWarning($"Попытка обновления токена не удалась: токен истек или невалиден для пользователя {user.Login}");
            throw new UnauthorizedAccessException("Неверный или истекший refresh токен");
        }

        _logger.LogInformation($"Refresh токен обновлен для пользователя {user.Login}");
        return await GenerateAuthResponseAsync(user, cancellationToken);  
    }

    public async Task<bool> LogoutAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == userId && !u.IsDeleted, 
                cancellationToken); 
        if (user == null)
        {
            _logger.LogWarning($"Попытка выхода не удалась: пользователь с ID {userId} не найден");
            return false;
        }
        
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _context.SaveChangesAsync(cancellationToken); 
        
        _logger.LogInformation($"Пользователь {user.Login} вышел из системы");
        return true;
    }

    public async Task<UserInfoDto> GetUserInfoAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == userId && !u.IsDeleted, 
                cancellationToken);  

        if (user == null)
        {
            _logger.LogWarning($"Запрос информации не удалась: пользователь с ID {userId} не найден");
            throw new KeyNotFoundException($"Пользователь с ID {userId} не найден");
        }
        
        var seller = await _context.Sellers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.UserId == user.Id && !s.IsDeleted, 
                cancellationToken);  // <-- Добавлен CancellationToken

        return new UserInfoDto
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email,
            FirstName = seller?.Name,
            LastName = seller?.Surname,
            IsActive = !user.IsDeleted,
            Role = user.Role
        };
    }

  

    private async Task<AuthResponseDto> GenerateAuthResponseAsync(
        User user, 
        CancellationToken cancellationToken = default)
    {
  
        var accessToken = _jwtService.GenerateAccessToken(user);

  
        var refreshToken = _jwtService.GenerateRefreshToken();
        
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(
            int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7")
        );
        await _context.SaveChangesAsync(cancellationToken); 
        
        var expiresInMinutes = int.Parse(_config["Jwt:AccessTokenExpiryMinutes"] ?? "15");
        var seller = await _context.Sellers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.UserId == user.Id && !s.IsDeleted, 
                cancellationToken); 
        
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = expiresInMinutes * 60,
            User = new UserInfoDto
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                FirstName = seller?.Name,
                LastName = seller?.Surname,
                IsActive = !user.IsDeleted,
                Role = user.Role
            }
        };
    }
}