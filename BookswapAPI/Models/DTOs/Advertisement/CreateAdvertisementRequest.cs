using System.ComponentModel.DataAnnotations;

namespace BookswapAPI.Models.DTOs.Advertisement;

public class CreateAdvertisementRequestDto
{
    [Required(ErrorMessage = "Название книги обязательно")]
    public string Title { get; set; }
    public string AuthorName { get; set; }
    public string Description { get; set; }
    public string BookTitle { get; set; }

    public Guid GenreId { get; set; }       

    public bool IsNew { get; set; }
    public string Condition { get; set; }
    public bool IsForever { get; set; }
    public bool IsPostamat { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string HouseNumber { get; set; }

    /// <summary>Ссылка на обложку из подсказок OpenLibrary (необязательно)</summary>
    public string? CoverUrl { get; set; }

    /// <summary>
    /// Не используется: владелец объявления определяется по JWT-токену авторизованного пользователя.
    /// Оставлено для совместимости со старыми клиентами.
    /// </summary>
    public Guid? OwnerId { get; set; }      
}

