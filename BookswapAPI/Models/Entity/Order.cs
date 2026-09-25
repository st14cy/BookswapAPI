using System.ComponentModel.DataAnnotations;
using BookswapAPI.Models.Enum;

namespace BookswapAPI.Models;

/// <summary>Оформленный заказ: книги, которые пользователь забирает</summary>
public class Order : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public OrderStatus Status { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
