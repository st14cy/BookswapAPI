using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookswapAPI.Models;

public class FavoriteAdvertisement: BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; }
    
    [Required]
    public Guid AdvertisementId { get; set; }
    public Advertisement Advertisement { get; set; }
    
    public DateTime AddedAt { get; set; } = DateTime.Now;
    
}