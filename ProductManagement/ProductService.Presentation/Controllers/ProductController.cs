using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    private int GetUserIdFromToken()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Invalid token."));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        int userId = GetUserIdFromToken();
        var products = await _productService.GetAllProductsForUserAsync(userId);
        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        int userId = GetUserIdFromToken();
        product.CreatorUserID = userId;
        await _productService.AddProductAsync(product);
        return CreatedAtAction(nameof(GetAll), new { id = product.ID }, product);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        int userId = GetUserIdFromToken();

        if (id != product.ID)
        {
            return BadRequest(new { Message = "Product ID in the body does not match the URL." });
        }
 
        await _productService.UpdateProductAsync(product, userId);

        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        int userId = GetUserIdFromToken();

        await _productService.DeleteProductAsync(id, userId);

        return NoContent();
    }
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? searchQuery,
        [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] bool? isAvailable)
    {
        int userId = GetUserIdFromToken();
        var products = await _productService.SearchProductsAsync(userId, searchQuery,
            minPrice, maxPrice, isAvailable);
        return Ok(products);
    }


}
