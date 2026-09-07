namespace BookswapAPI.Models.DTOs.Advertisement;

public class UpdateAdvertisementRequestDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Author { get; set; }
    public Guid? GenreId { get; set; }
    public bool? IsNew { get; set; }
    public bool? IsForever { get; set; }
    public bool? IsPostamat { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string HouseNumber { get; set; }
    
}