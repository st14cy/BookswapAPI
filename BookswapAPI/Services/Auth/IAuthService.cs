using BookswapAPI.Models.DTOs;

namespace BookswapAPI.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(
        RegisterDto dto, 
        CancellationToken cancellationToken = default);
    
    Task<AuthResponseDto> LoginAsync(
        LoginDto dto, 
        CancellationToken cancellationToken = default);
    
    Task<AuthResponseDto> RefreshTokenAsync(
        string refreshToken, 
        CancellationToken cancellationToken = default);
    
    Task<bool> LogoutAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
    
    Task<UserInfoDto> GetUserInfoAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
}