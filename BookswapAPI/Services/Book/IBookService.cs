using BookswapAPI.Models.DTOs.Book;

namespace BookswapAPI.Services.Book;

public interface IBookService
{
    
    Task<List<BookDto>> SearchBooksAsync(string query, int limit = 10, int page = 1);
    
   
    Task<List<BookDto>> SearchByTitleAsync(string title, int limit = 10);
    
   
    Task<List<BookDto>> SearchByAuthorAsync(string author, int limit = 10);
    
    /// <summary>
    /// Получение книги по ISBN
    /// </summary>
    Task<BookDto> GetBookByIsbnAsync(string isbn);
    
    /// <summary>
    /// Получение URL обложки
    /// </summary>
    string GetCoverUrl(int? coverId, string size = "M");
    
    /// <summary>
    /// Получение обложки как byte[]
    /// </summary>
    Task<byte[]> GetCoverImageAsync(int coverId, string size = "M");
    
    /// <summary>
    /// Получение количества найденных книг
    /// </summary>
    Task<int> GetTotalCountAsync(string query);
}