using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace BookswapAPI.Models;

public class Advertisement : BaseEntity
{ 
    [Required(ErrorMessage = "Заголовок не может быть пустым")]
    public string Title { get; set; }
    
    [Required]
    public Guid BookId { get; set; }
    public Book Book { get; set; }
    [Required]
    public Guid SellerId { get; set; }
    public Seller Seller { get; set; }
    
    public bool IsForever { get; set; }
    public bool IsPostamat { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public string Description { get; set; }
    
    public string City { get; set; }
    public string HouseNumber { get; set; } // Вместо NumHome
    public string Street { get; set; }
    
    public bool IsActive { get; set; } 
    
    public int ViewsCount { get; set; }
    public int LikeCount { get; set; }
    
    public bool IsNew { get; set; } 
    
}