using BookswapAPI.Services.Book;
using Microsoft.AspNetCore.Mvc;

namespace BookswapAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchBooks(
        [FromQuery] string q,
        [FromQuery] int limit = 10,
        [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "Введите поисковый запрос" });

        var books = await _bookService.SearchBooksAsync(q, limit, page);
        var total = await _bookService.GetTotalCountAsync(q);

        return Ok(new
        {
            total,
            page,
            limit,
            books
        });
    }

    [HttpGet("isbn/{isbn}")]
    public async Task<IActionResult> GetBookByIsbn(string isbn)
    {
        var book = await _bookService.GetBookByIsbnAsync(isbn);

        if (book == null)
            return NotFound(new { error = "Книга не найдена" });

        return Ok(book);
    }

    [HttpGet("cover/{coverId}")]
    public async Task<IActionResult> GetCover(int coverId, [FromQuery] string size = "M")
    {
        var image = await _bookService.GetCoverImageAsync(coverId, size);

        if (image == null)
            return NotFound();

        return File(image, "image/jpeg");
    }
}