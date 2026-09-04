using System.ComponentModel.DataAnnotations;

namespace BookswapAPI.Models;

public class Book : BaseEntity
{ 
    public string Title { get; set; }
    public string Author { get; set; }
    
    [Required]
    public int GenreId { get; set; }
    public Genre Genre { get; set; }
}