using BookswapAPI.Services.Genres;
using Microsoft.AspNetCore.Mvc;

namespace BookswapAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenreController(IGenreService genreService) : ControllerBase
{
    /// <summary>
    /// Список жанров для выпадающего списка
    /// </summary>
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var genres = await genreService.GetAllAsync(cancellationToken);
        return Ok(genres);
    }
}
