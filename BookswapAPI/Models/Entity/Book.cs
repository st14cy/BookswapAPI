using System.ComponentModel.DataAnnotations;

namespace BookswapAPI.Models;

public class Book : BaseEntity
{  
    [Required(ErrorMessage = "Название книги не может быть пустым")]
    public string Title { get; set; }
    [Required(ErrorMessage = "Автор книги не может быть пустым")]
    public string Author { get; set; }
    
    [Required]
    public Guid GenreId { get; set; }
    public Genre Genre { get; set; }
}