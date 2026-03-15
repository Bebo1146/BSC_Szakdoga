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

        // NEW: products the current user has placed bids on
        [HttpGet("my-bids")]
        public async Task<IActionResult> GetProductsIBidOn()
        {
            string bidderId = JwtClaimReader.GetNameFromJwt(JwtClaimReader.GetTokenFromRequest(Request));
            if (string.IsNullOrEmpty(bidderId))
            {
                return Unauthorized("User ID not found in token");
            }

            IEnumerable<ProductDto> products = await _productService.GetProductsByBidderAsync(bidderId);
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

        // Bid request DTO
        public record BidRequest(decimal Amount);

        /// <summary>
        /// Place a bid on a product.
        /// </summary>
        [HttpPost("{id}/bid")]
        public async Task<IActionResult> PlaceBid(string id, [FromBody] BidRequest? request)
        {
            if (request is null)
            {
                return BadRequest("Invalid bid request.");
            }

            if (request.Amount <= 0m)
            {
                return BadRequest("Bid amount must be greater than zero.");
            }

            string bidderId = JwtClaimReader.GetNameFromJwt(JwtClaimReader.GetTokenFromRequest(Request));
            if (string.IsNullOrEmpty(bidderId))
            {
                return Unauthorized("User ID not found in token");
            }

            string bidderUsername = JwtClaimReader.GetPreferredNameFromJwt(JwtClaimReader.GetTokenFromRequest(Request)) ?? bidderId;

            var (success, error, bid) = await _productService.PlaceBidAsync(id, request.Amount, bidderId, bidderUsername);
            if (!success)
            {
                return BadRequest(new { Message = "Bid failed", Reason = error });
            }

            // Return the updated product and the created bid
            ProductDto? updatedProduct = await _productService.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id = id }, new { Product = updatedProduct, Bid = bid });
        }

        [HttpGet("/health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok("OK");
        }
    }
}
