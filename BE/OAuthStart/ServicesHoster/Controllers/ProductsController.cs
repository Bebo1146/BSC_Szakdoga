using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ServicesHoster.Services;
using TokenValidation.TokenValidation;

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
            IEnumerable<ProductDto> products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpGet("my-products")]
        public async Task<IActionResult> GetMyProducts()
        {
            string userName = JwtClaimReader.GetNameFromJwt(JwtClaimReader.GetTokenFromRequest(Request));
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("User ID not found in token");
            }

            IEnumerable<ProductDto> products = await _productService.GetByUserAsync(userName);
            return Ok(products);
        }

        [HttpPost("addMultiple")]
        public async Task<IActionResult> AddMultiple([FromBody] List<ProductDto>? products)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string[]> errors = ModelState
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

            string userName = JwtClaimReader.GetNameFromJwt(JwtClaimReader.GetTokenFromRequest(Request));
            if (string.IsNullOrEmpty(userName))
            {
                return Unauthorized("User ID not found in token");
            }

            await _productService.AddRangeAsync(products, userName, JwtClaimReader.GetPreferredNameFromJwt(JwtClaimReader.GetTokenFromRequest(Request)));
            return CreatedAtAction(nameof(GetAll), new { count = products.Count }, products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            ProductDto? product = await _productService.GetByIdAsync(id);
            return product is null ? NotFound() : Ok(product);
        }

        [HttpGet("/health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok("OK");
        }
    }
}
