using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookswapAPI.Models;

public class Seller : BaseEntity
{
    [Required(ErrorMessage = "Имя не может быть пустым")]
    public string Name { get; set; }
    public string Surname { get; set; }
    public double Rating { get; set; }
    public string Image { get; set; }
    
    public Guid UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; }
        
    public ICollection<Advertisement> Advertisements { get; set; }
}