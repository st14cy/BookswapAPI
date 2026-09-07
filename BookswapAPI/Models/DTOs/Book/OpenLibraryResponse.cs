using System.Text.Json.Serialization;

namespace BookswapAPI.Models.DTOs.Book;

public class OpenLibraryResponse
{
    [JsonPropertyName("numFound")]
    public int NumFound { get; set; }
    
    [JsonPropertyName("docs")]
    public List<BookDoc> Docs { get; set; }
}