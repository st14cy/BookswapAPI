using BookswapAPI.Models.DTOs.Cart;

namespace BookswapAPI.Services.Carts;

public interface ICartService
{
    /// <summary>Содержимое корзины, новые сверху</summary>
    Task<IReadOnlyList<CartItemDto>> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Id объявлений в корзине — чтобы показать «В корзине» на странице книги</summary>
    Task<IReadOnlyList<Guid>> GetIdsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>«Забрать книгу». Повторное добавление ничего не меняет.</summary>
    /// <exception cref="KeyNotFoundException">объявление не найдено или снято с публикации</exception>
    /// <exception cref="InvalidOperationException">это объявление самого пользователя</exception>
    Task AddAsync(Guid userId, Guid advertisementId, CancellationToken cancellationToken = default);

    /// <summary>Убрать из корзины (мягко: IsDeleted = true)</summary>
    Task RemoveAsync(Guid userId, Guid advertisementId, CancellationToken cancellationToken = default);

    /// <summary>
    /// «Оформить»: создаёт заказ из всех книг корзины, очищает корзину
    /// и снимает объявления с публикации (книгу забрали — другим она больше недоступна).
    /// </summary>
    /// <exception cref="InvalidOperationException">корзина пуста или какие-то книги уже недоступны</exception>
    Task<OrderDto> CheckoutAsync(Guid userId, CancellationToken cancellationToken = default);
}
