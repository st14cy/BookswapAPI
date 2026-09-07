using BookswapAPI.Models.Enum;

namespace BookswapAPI.Models.DTOs;

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
    public Roles Role { get; set; }
}