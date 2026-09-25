using BookswapAPI.Services.Sellers;
using Microsoft.AspNetCore.Mvc;

namespace BookswapAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SellerController(ISellerService sellerService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await sellerService.GetByIdAsync(id, cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
