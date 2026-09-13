using BookswapAPI.Services.Advertisement;
using Microsoft.AspNetCore.Mvc;

namespace BookswapAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdvertisementController : ControllerBase
{
    public AdvertisementController(IAdvertisementService advertisementService)
    {
        _advertisementService = advertisementService;
    }
    
    private readonly IAdvertisementService  _advertisementService;

    [HttpGet("getAll")]
    public async Task<IActionResult> GetAll()
    {
        var advertisement=_advertisementService.GeAllAdvertisementAsync();
        return Ok(await advertisement);
    }
    
}