using System.ComponentModel.DataAnnotations;

namespace BookswapAPI.Models.DTOs;

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; }
}