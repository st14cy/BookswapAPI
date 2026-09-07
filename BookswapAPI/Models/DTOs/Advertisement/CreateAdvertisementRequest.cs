using System.ComponentModel.DataAnnotations;

namespace BookswapAPI.Models.DTOs.Advertisement;

public class CreateAdvertisementRequestDto
{
    [Required(ErrorMessage = "Название книги обязательно")]
    public string Title { get; set; }
    public string Author { get; set; }
    public string Description { get; set; }
    
    [Required(ErrorMessage = "ID жанра обязателен")]
    public Guid GenreId { get; set; }
    
    public bool IsNew { get; set; }
    public string Condition { get; set; }
    public bool IsForever { get; set; }
    public bool IsPostamat { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string HouseNumber { get; set; }
    
    [Required(ErrorMessage = "ID владельца обязателен")]
    public Guid OwnerId { get; set; }
}