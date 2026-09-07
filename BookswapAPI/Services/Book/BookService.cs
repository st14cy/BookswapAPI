using System.Text.Json;
using BookswapAPI.Models.DTOs.Book;

namespace BookswapAPI.Services.Book;

public class BookService : IBookService
{
    private readonly HttpClient _httpClient;

    public BookService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<BookDto>> SearchBooksAsync(string query, int limit = 10, int page = 1)
    {
        try
        {
            var url = $"search.json?q={Uri.EscapeDataString(query)}&limit={limit}&offset={(page - 1) * limit}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            var data = JsonSerializer.Deserialize<OpenLibraryResponse>(json, options);

            if (data?.Docs == null)
                return [];

            return data.Docs.Select(doc => new BookDto
            {
                Title = doc.Title ?? "Без названия",
                Author = doc.AuthorNames?.FirstOrDefault() ?? "Неизвестный автор",
                Year = doc.FirstPublishYear?.ToString() ?? "Не указан",
                Publisher = doc.Publishers?.FirstOrDefault() ?? "Не указан",
                CoverId = doc.CoverId,
                Isbn = doc.Isbns?.FirstOrDefault(),
                Pages = doc.Pages,
                Description = doc.Description,
                CoverUrl = GetCoverUrl(doc.CoverId)
            }).ToList();
        }
        catch (HttpRequestException)
        {
            return new List<BookDto>();
        }
    }

    public async Task<List<BookDto>> SearchByTitleAsync(string title, int limit = 10)
    {
        return await SearchBooksAsync($"title:{title}", limit);
    }

    public async Task<List<BookDto>> SearchByAuthorAsync(string author, int limit = 10)
    {
        return await SearchBooksAsync($"author:{author}", limit);
    }

    public async Task<BookDto> GetBookByIsbnAsync(string isbn)
    {
        try
        {
            var response = await _httpClient.GetAsync($"isbn/{isbn}.json");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var doc = JsonSerializer.Deserialize<BookDoc>(json, options);

            if (doc == null)
                return null;

            return new BookDto
            {
                Title = doc.Title ?? "Без названия",
                Author = doc.AuthorNames?.FirstOrDefault() ?? "Неизвестный автор",
                Year = doc.FirstPublishYear?.ToString() ?? "Не указан",
                Publisher = doc.Publishers?.FirstOrDefault() ?? "Не указан",
                CoverId = doc.CoverId,
                Isbn = isbn,
                Pages = doc.Pages,
                Description = doc.Description,
                CoverUrl = GetCoverUrl(doc.CoverId)
            };
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    public string GetCoverUrl(int? coverId, string size = "M")
    {
        if (!coverId.HasValue)
            return null;

        return $"https://covers.openlibrary.org/b/id/{coverId}-{size}.jpg";
    }

    public async Task<byte[]> GetCoverImageAsync(int coverId, string size = "M")
    {
        try
        {
            var url = GetCoverUrl(coverId, size);
            if (string.IsNullOrEmpty(url))
                return null;

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch
        {
            return null;
        }
    }

    public async Task<int> GetTotalCountAsync(string query)
    {
        try
        {
            var url = $"search.json?q={Uri.EscapeDataString(query)}&limit=0";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<OpenLibraryResponse>(json, options);

            return data?.NumFound ?? 0;
        }
        catch
        {
            return 0;
        }
    }
}