using System.Security.Claims;
using BookswapAPI.Models;

namespace BookswapAPI.Services.JWT;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    (bool isValid, string? userId) ValidateRefreshToken(User user, string refreshToken);
}