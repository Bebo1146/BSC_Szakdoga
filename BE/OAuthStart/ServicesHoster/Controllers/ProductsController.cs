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

        [HttpPost("mark-sold")]
        public async Task<IActionResult> MarkAsSold([FromBody] List<string>? ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("No product IDs provided.");
            }

            List<ProductDto> updatedProducts = [];
            List<object> failedProducts = [];

            foreach (string id in ids.Where(id => !string.IsNullOrWhiteSpace(id)))
            {
                var (success, error, product) = await _productService.MarkAsSoldAsync(id);
                if (success && product is not null)
                {
                    updatedProducts.Add(product);
                }
                else
                {
                    failedProducts.Add(new { Id = id, Message = error });
                }
            }

            return Ok(new
            {
                UpdatedProducts = updatedProducts,
                FailedProducts = failedProducts
            });
        }

        [HttpPost("mark-rejected")]
        public async Task<IActionResult> MarkAsRejected([FromBody] List<RejectProductRequest>? requests)
        {
            if (requests == null || requests.Count == 0)
            {
                return BadRequest("No rejection requests provided.");
            }

            List<ProductDto> updatedProducts = [];
            List<object> failedProducts = [];

            foreach (RejectProductRequest request in requests.Where(r => !string.IsNullOrWhiteSpace(r.Id)))
            {
                var (success, error, product) = await _productService.MarkAsRejectedAsync(request.Id, request.Reason);
                if (success && product is not null)
                {
                    updatedProducts.Add(product);
                }
                else
                {
                    failedProducts.Add(new { request.Id, Message = error });
                }
            }

            return Ok(new
            {
                UpdatedProducts = updatedProducts,
                FailedProducts = failedProducts
            });
        }

        [HttpPost("mark-accepted")]
        public async Task<IActionResult> MarkAsAccepted([FromBody] List<string>? ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("No product IDs provided.");
            }

            List<ProductDto> updatedProducts = [];
            List<object> failedProducts = [];

            foreach (string id in ids.Where(id => !string.IsNullOrWhiteSpace(id)))
            {
                var (success, error, product) = await _productService.MarkAsAcceptedAsync(id);
                if (success && product is not null)
                {
                    updatedProducts.Add(product);
                }
                else
                {
                    failedProducts.Add(new { Id = id, Message = error });
                }
            }

            return Ok(new
            {
                UpdatedProducts = updatedProducts,
                FailedProducts = failedProducts
            });
        }

        [HttpPost("{id}/feedback")]
        public async Task<IActionResult> AddFeedback(string id, [FromBody] FeedbackDto? feedback)
        {
            if (feedback is null)
            {
                return BadRequest("Invalid feedback.");
            }

            if (!feedback.Rating.HasValue || feedback.Rating < 1 || feedback.Rating > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            var (success, error, product) = await _productService.AddFeedbackAsync(id, feedback);
            if (!success)
            {
                return BadRequest(new { Message = error });
            }

            return Ok(product);
        }

        [HttpGet("my-received-feedback")]
        public async Task<IActionResult> GetMyReceivedFeedback()
        {
            string userId = JwtClaimReader.GetNameFromJwt(JwtClaimReader.GetTokenFromRequest(Request));
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            IEnumerable<FeedbackItemDto> feedbackItems = await _productService.GetFeedbackReceivedByUserAsync(userId);
            return Ok(feedbackItems);
        }

        [HttpGet("/health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok("OK");
        }
    }
}
