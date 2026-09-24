namespace BookswapAPI.Models.DTOs.Book;

/// <summary>
/// Подсказка для автозаполнения полей «Название книги» и «Автор»
/// </summary>
public class BookSuggestionDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Year { get; set; }
    public string? CoverUrl { get; set; }
}
