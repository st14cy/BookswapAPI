namespace BookswapAPI.Models.DTOs.Book;

public class BookDto
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string Year { get; set; }
    public string Publisher { get; set; }
    public int? CoverId { get; set; }
    public string CoverUrl { get; set; }
    public string Isbn { get; set; }
    public int? Pages { get; set; }
    public string Description { get; set; }
}