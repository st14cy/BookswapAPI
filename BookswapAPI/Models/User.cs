using System.ComponentModel.DataAnnotations;
using BookswapAPI.Models.Enum;

namespace BookswapAPI.Models;

public class User : BaseEntity
{
    [Required(ErrorMessage = "Логин не может быть пустым")]
    public string Login { get; set; }
    [Required(ErrorMessage = "Пароль не может быть пустым")]
    public string Password { get; set; }
    
    public Roles Role { get; set; }
}