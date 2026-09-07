using BookswapAPI.Data;
using BookswapAPI.Models.DTOs.Advertisement;
using Microsoft.EntityFrameworkCore;

namespace BookswapAPI.Services.Advertisement;

public class AdvertisementService : IAdvertisementService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdvertisementService> _logger;


    public async Task<IEnumerable<AdvertisementInfoDto>> GeAllAdvertisementAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var res= await _context.Advertisements
                .Include(x=>x.Seller)
                .Where(x=>!x.IsDeleted).Select(x=>new AdvertisementInfoDto
                {
                    Id =  x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    AuthorName = x.Author,
                    BookTitle = x.Title,
                    GenreId=x.GenreId,
                    GenreName = x.Genre.Name,
                    IsNew = x.IsNew,
                    IsForever =  x.IsForever,
                    IsPostamat = x.IsPostamat,
                    City =  x.City,
                    Street = x.Street,
                    HouseNumber = x.HouseNumber,
                    OwnerId = x.SellerId,
                    OwnerName =x.Seller.Name
                }).ToListAsync(cancellationToken);
            return res;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Не удалось выгрузить объявления");
            throw;
        }
    }

  public async Task<AdvertisementInfoDto> GetAdvertisementByIdAsync(
    Guid id, 
    CancellationToken cancellationToken = default)
{
    try
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        var advertisement = await _context.Advertisements
            .Include(x => x.Seller)
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new AdvertisementInfoDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                
                BookTitle = x.Title ,
                AuthorName =x.Author ,
                
                GenreId =x.GenreId,
                GenreName = x.Genre.Name,
                IsNew = x.IsNew,
                IsForever = x.IsForever,
                IsPostamat = x.IsPostamat,
                
                City = x.City,
                Street = x.Street,
                HouseNumber = x.HouseNumber,
                
                OwnerId = x.SellerId,
                OwnerName = x.Seller != null ? x.Seller.Name : "Неизвестно",
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (advertisement != null) return advertisement;
        _logger.LogWarning("Объявление с ID {AdId} не найдено", id);
        throw new KeyNotFoundException($"Объявление с ID {id} не найдено");
    }
    catch (OperationCanceledException)
    {
        _logger.LogWarning("Запрос GetAdvertisementById для ID {AdId} был отменен", id);
        throw;
    }
    catch (Exception e)
    {
        _logger.LogError(e, "Ошибка при получении объявления с ID {AdId}", id);
        throw;
    }
}

    public Task<AdvertisementInfoDto> CreateAdvertisementAsync(CreateAdvertisementRequestDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<AdvertisementInfoDto> UpdateAdvertisementAsync(Guid id, UpdateAdvertisementRequestDto request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAdvertisementAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AdvertisementInfoDto>> GetAdvertisementByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<AdvertisementInfoDto>> SearchAdvertisementAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}