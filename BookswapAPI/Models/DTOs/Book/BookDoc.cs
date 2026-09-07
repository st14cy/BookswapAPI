using System.Text.Json.Serialization;

namespace BookswapAPI.Models.DTOs.Book;

public class BookDoc
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("author_name")]
    public List<string> AuthorNames { get; set; }
    
    [JsonPropertyName("first_publish_year")]
    public int? FirstPublishYear { get; set; }
    
    [JsonPropertyName("publisher")]
    public List<string> Publishers { get; set; }
    
    [JsonPropertyName("cover_i")]
    public int? CoverId { get; set; }
    
    [JsonPropertyName("isbn")]
    public List<string> Isbns { get; set; }
    
    [JsonPropertyName("number_of_pages")]
    public int? Pages { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
}