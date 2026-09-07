using System.ComponentModel.DataAnnotations;

namespace BookswapAPI.Models.DTOs;

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string Login { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}