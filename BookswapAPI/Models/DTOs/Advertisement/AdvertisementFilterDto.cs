namespace BookswapAPI.Models.DTOs.Advertisement;

public class AdvertisementFilterDto
{
    public string SearchTerm { get; set; }
    public Guid? GenreId { get; set; }
    public string? Author { get; set; }
    public bool? IsNew { get; set; }
    public bool? IsForever { get; set; }
    public bool? IsPostamat { get; set; }
    public string City { get; set; }
}