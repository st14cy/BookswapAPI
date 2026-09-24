using BookswapAPI.Data;
using BookswapAPI.Models.DTOs.Genres;
using Microsoft.EntityFrameworkCore;

namespace BookswapAPI.Services.Genres;

public class GenreService(AppDbContext context) : IGenreService
{
    public async Task<IReadOnlyList<GenreDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Genres
            .AsNoTracking()
            .Where(g => !g.IsDeleted)
            .OrderBy(g => g.Name)
            .Select(g => new GenreDto { Id = g.Id, Name = g.Name })
            .ToListAsync(cancellationToken);
    }
}
