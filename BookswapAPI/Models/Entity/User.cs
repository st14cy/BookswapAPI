using System.ComponentModel.DataAnnotations;
using BookswapAPI.Models.Enum;

namespace BookswapAPI.Models;

public class User : BaseEntity
{
    [Required(ErrorMessage = "Логин не может быть пустым")]
    public string Login { get; set; }
    [Required(ErrorMessage = "Email не может быть пустым")]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; }
    [Required(ErrorMessage = "Пароль не может быть пустым")]
    [MaxLength(500)]
    public string PasswordHash { get; set; }
    [MaxLength(500)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }
    
    public Roles Role { get; set; }
}