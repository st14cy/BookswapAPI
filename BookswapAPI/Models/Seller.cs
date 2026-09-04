using System.ComponentModel.DataAnnotations;

namespace BookswapAPI.Models;

public class Seller : BaseEntity
{
    [Required(ErrorMessage = "Имя не может быть пустым")]
    public string Name { get; set; }
    public string Surname { get; set; }
    public double Rating { get; set; }
    public string Image { get; set; }
}