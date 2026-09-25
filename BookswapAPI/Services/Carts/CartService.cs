using BookswapAPI.Data;
using BookswapAPI.Models;
using BookswapAPI.Models.DTOs.Advertisement;
using BookswapAPI.Models.DTOs.Cart;
using BookswapAPI.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace BookswapAPI.Services.Carts;

public class CartService(AppDbContext context, ILogger<CartService> logger) : ICartService
{
    public async Task<IReadOnlyList<CartItemDto>> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.CartItems
            .AsNoTracking()
            .Where(c => c.UserId == userId && !c.IsDeleted && !c.Advertisement.IsDeleted)
            .OrderByDescending(c => c.AddedAt)
            .Select(c => new CartItemDto
            {
                AddedAt = c.AddedAt,
                Advertisement = new AdvertisementInfoDto
                {
                    Id = c.Advertisement.Id,
                    Title = c.Advertisement.Title,
                    Description = c.Advertisement.Description,
                    AuthorName = c.Advertisement.Author,
                    BookTitle = c.Advertisement.TitleBook,
                    GenreId = c.Advertisement.GenreId,
                    GenreName = c.Advertisement.Genre.Name,
                    IsNew = c.Advertisement.IsNew,
                    IsForever = c.Advertisement.IsForever,
                    IsPostamat = c.Advertisement.IsPostamat,
                    City = c.Advertisement.City,
                    Street = c.Advertisement.Street,
                    HouseNumber = c.Advertisement.HouseNumber,
                    OwnerId = c.Advertisement.SellerId,
                    OwnerName = c.Advertisement.Seller.Name,
                    CreatedAt = c.Advertisement.CreatedAt,
                    IsActive = c.Advertisement.IsActive,
                    CoverUrl = c.Advertisement.CoverUrl,
                    LikeCount = c.Advertisement.LikeCount
                }
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.CartItems
            .AsNoTracking()
            .Where(c => c.UserId == userId && !c.IsDeleted && !c.Advertisement.IsDeleted)
            .Select(c => c.AdvertisementId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Guid userId, Guid advertisementId, CancellationToken cancellationToken = default)
    {
        var advertisement = await context.Advertisements
            .Include(a => a.Seller)
            .FirstOrDefaultAsync(a => a.Id == advertisementId && !a.IsDeleted && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Объявление не найдено или уже недоступно");

        if (advertisement.Seller.UserId == userId)
            throw new InvalidOperationException("Нельзя забрать свою книгу");

        var item = await context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.AdvertisementId == advertisementId, cancellationToken);

        var now = DateTime.UtcNow;
        if (item == null)
        {
            await context.CartItems.AddAsync(new CartItem
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AdvertisementId = advertisementId,
                AddedAt = now,
                CreatedAt = now,
                CreatedBy = userId
            }, cancellationToken);
        }
        else if (item.IsDeleted)
        {
            item.IsDeleted = false;
            item.DeletedAt = null;
            item.DeletedBy = null;
            item.AddedAt = now;
            item.UpdatedAt = now;
            item.UpdatedBy = userId;
        }
        else
        {
            return;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Guid userId, Guid advertisementId, CancellationToken cancellationToken = default)
    {
        var item = await context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.AdvertisementId == advertisementId && !c.IsDeleted,
                cancellationToken);
        if (item == null) return;

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.DeletedBy = userId;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<OrderDto> CheckoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var items = await context.CartItems
            .Include(c => c.Advertisement)
            .ThenInclude(a => a.Seller)
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
            throw new InvalidOperationException("Корзина пуста");

        var unavailable = items
            .Where(c => c.Advertisement.IsDeleted || !c.Advertisement.IsActive)
            .Select(c => c.Advertisement.TitleBook)
            .ToList();
        if (unavailable.Count > 0)
            throw new InvalidOperationException(
                $"Эти книги уже недоступны, уберите их из корзины: {string.Join(", ", unavailable)}");

        var buyerName = await GetDisplayNameAsync(userId, cancellationToken);

        var now = DateTime.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.Created,
            CreatedAt = now,
            CreatedBy = userId
        };

        foreach (var item in items)
        {
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                AdvertisementId = item.AdvertisementId,
                SellerId = item.Advertisement.SellerId,
                BookTitle = item.Advertisement.TitleBook,
                Author = item.Advertisement.Author,
                CreatedAt = now,
                CreatedBy = userId
            });

            item.IsDeleted = true;
            item.DeletedAt = now;
            item.DeletedBy = userId;

            item.Advertisement.IsActive = false;
            item.Advertisement.EndDate = now;
            item.Advertisement.UpdatedAt = now;

            await context.Notifications.AddAsync(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = item.Advertisement.Seller.UserId,
                Type = NotificationType.BookTaken,
                Message = Truncate($"{buyerName} забрал(а) вашу книгу «{item.Advertisement.TitleBook}»", 500),
                AdvertisementId = item.AdvertisementId,
                OrderId = order.Id,
                IsRead = false,
                CreatedAt = now,
                CreatedBy = userId
            }, cancellationToken);
        }

        await context.Orders.AddAsync(order, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation("Пользователь {UserId} оформил заказ {OrderId} ({Count} книг)", userId, order.Id, order.Items.Count);

        var result = new OrderDto
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            Items = order.Items.Select(i => new OrderItemDto
            {
                AdvertisementId = i.AdvertisementId,
                BookTitle = i.BookTitle,
                Author = i.Author
            }).ToList()
        };
        return result;
    }

    private async Task<string> GetDisplayNameAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sellerName = await context.Sellers
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .Select(s => s.Name)
            .FirstOrDefaultAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(sellerName)) return sellerName;

        var login = await context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Login)
            .FirstOrDefaultAsync(cancellationToken);
        return login ?? "Пользователь";
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..(maxLength - 1)] + "…";
}
