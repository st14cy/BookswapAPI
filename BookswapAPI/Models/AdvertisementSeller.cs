using System.ComponentModel.DataAnnotations;

namespace BookswapAPI.Models;

public class AdvertisementSeller : BaseEntity
{
    [Required]
    public int SellerId { get; set; }
    public Seller Seller { get; set; }
    
    [Required]
    public int AdvertisementId { get; set; }
    public Advertisement Advertisement { get; set; }
}