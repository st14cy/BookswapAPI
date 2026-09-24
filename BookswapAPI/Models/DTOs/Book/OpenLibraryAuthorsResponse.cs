using System.Text.Json.Serialization;

namespace BookswapAPI.Models.DTOs.Book;

/// <summary>
/// Ответ https://openlibrary.org/search/authors.json
/// </summary>
public class OpenLibraryAuthorsResponse
{
    [JsonPropertyName("docs")]
    public List<OpenLibraryAuthorDoc>? Docs { get; set; }
}

public class OpenLibraryAuthorDoc
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("top_work")]
    public string? TopWork { get; set; }

    [JsonPropertyName("work_count")]
    public int? WorkCount { get; set; }
}
