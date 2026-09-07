using System.ComponentModel.DataAnnotations;

namespace BookswapAPI.Models;

public class Genre : BaseEntity
{
    [Required(ErrorMessage = "Название жанра не может быть пустым")]
    public string Name { get; set; }
    
}