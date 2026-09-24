using BookswapAPI.Models.DTOs.Genres;

namespace BookswapAPI.Services.Genres;

public interface IGenreService
{
    /// <summary>
    /// Все (не удалённые) жанры, отсортированные по названию
    /// </summary>
    Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
