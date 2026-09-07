namespace BookswapAPI.Models.DTOs.Advertisement;

public class AdvertisementInfoDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string AuthorName { get; set; }
    public string BookTitle { get; set; }
    public Guid GenreId { get; set; }
    public string GenreName { get; set; }
    public bool IsNew { get; set; } 
    public bool IsForever { get; set; }  
    public bool IsPostamat { get; set; } 
    public string City { get; set; }
    public string Street { get; set; }
    public string HouseNumber { get; set; } // Вместо NumHome
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; }
    
    public DateTime CreatedAt { get; set; }
    

}