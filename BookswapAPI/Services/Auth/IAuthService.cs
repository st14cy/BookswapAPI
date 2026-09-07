using BookswapAPI.Models.DTOs;

namespace BookswapAPI.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> LogoutAsync(Guid userId);
    Task<UserInfoDto> GetUserInfoAsync(Guid userId);
}