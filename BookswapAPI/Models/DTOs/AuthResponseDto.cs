namespace BookswapAPI.Models.DTOs;

public class AuthResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int ExpiresIn { get; set; } // секунды
    public UserInfoDto User { get; set; }
}