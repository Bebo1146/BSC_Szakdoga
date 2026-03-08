using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ServicesHoster.Services;
using System.Security.Claims;

namespace ServicesHoster.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableCors("AllowAll")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("my-products")]
        public async Task<IActionResult> GetMyProducts()
        {
            //var userId = GetCurrentUserId();
            var userId = "user123";
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var products = await _productService.GetByUserAsync(userId);
            return Ok(products);
        }

        [HttpPost("addMultiple")]
        public async Task<IActionResult> AddMultiple([FromBody] List<ProductDto>? products)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                return BadRequest(new { Message = "Validation failed", Errors = errors });
            }

            if (products == null || products.Count == 0)
            {
                return BadRequest("No products provided.");
            }

            //var userId = GetCurrentUserId();
            var userId = "user123";
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            await _productService.AddRangeAsync(products, userId);
            return CreatedAtAction(nameof(GetAll), new { count = products.Count }, products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var product = await _productService.GetByIdAsync(id);
            return product is null ? NotFound() : Ok(product);
        }

        [HttpGet("/health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok("OK");
        }

        // Helper method to extract user ID from JWT token
        private string? GetCurrentUserId()
        {
            // Try different claim types that Keycloak might use
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("preferred_username")?.Value
                ?? User.FindFirst("email")?.Value;
        }

        // Optional: Get detailed user info for debugging
        [HttpGet("user-info")]
        public IActionResult GetUserInfo()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(new
            {
                UserId = GetCurrentUserId(),
                Claims = claims
            });
        }
    }
}
