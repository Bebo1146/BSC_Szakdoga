using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using ServicesHoster.Controllers;
using ServicesHoster.Services;

namespace Tests
{
    [TestFixture]
    public class ProductsControllerWithJwtTests
    {
        private Mock<IProductService> _mockService = null!;
        private ProductsController _controller = null!;

        private static string CreateTestJwt(string name, string preferredUsername)
        {
            JwtSecurityTokenHandler handler = new();
            SecurityTokenDescriptor descriptor = new()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("name", name),
                    new Claim("preferred_username", preferredUsername)
                }),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(new byte[32]),
                    SecurityAlgorithms.HmacSha256)
            };
            SecurityToken token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        private void SetupControllerWithJwt(string name, string preferredUsername)
        {
            _mockService = new Mock<IProductService>();
            _controller = new ProductsController(_mockService.Object);

            string jwt = CreateTestJwt(name, preferredUsername);
            DefaultHttpContext httpContext = new();
            httpContext.Request.Headers["Authorization"] = $"Bearer {jwt}";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        private void SetupControllerWithoutJwt()
        {
            _mockService = new Mock<IProductService>();
            _controller = new ProductsController(_mockService.Object);

            DefaultHttpContext httpContext = new();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Test]
        public async Task GetMyProducts_WithValidJwt_ReturnsOkWithProducts()
        {
            SetupControllerWithJwt("Beni Beni", "Beni");
            List<ProductDto> products = new() { new ProductDto { Id = "p-1", SellerId = "Beni Beni" } };
            _mockService.Setup(s => s.GetByUserAsync("Beni Beni")).ReturnsAsync(products);

            IActionResult result = await _controller.GetMyProducts();

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.EqualTo(products));
        }

        [Test]
        public async Task GetMyProducts_NoJwt_ReturnsUnauthorized()
        {
            SetupControllerWithoutJwt();

            IActionResult result = await _controller.GetMyProducts();

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task GetProductsIBidOn_WithValidJwt_ReturnsOk()
        {
            SetupControllerWithJwt("Boti Boti", "Boti");
            List<ProductDto> products = new() { new ProductDto { Id = "p-2" } };
            _mockService.Setup(s => s.GetProductsByBidderAsync("Boti Boti")).ReturnsAsync(products);

            IActionResult result = await _controller.GetProductsIBidOn();

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
        }

        [Test]
        public async Task GetProductsIBidOn_NoJwt_ReturnsUnauthorized()
        {
            SetupControllerWithoutJwt();

            IActionResult result = await _controller.GetProductsIBidOn();

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task AddMultiple_WithValidJwt_ReturnsCreated()
        {
            SetupControllerWithJwt("Seller Name", "seller1");
            List<ProductDto> products = new()
            {
                new ProductDto { Name = "New Product", StartingPrice = 50 }
            };

            _mockService.Setup(s => s.AddRangeAsync(products, "Seller Name", "seller1"))
                .Returns(Task.CompletedTask);

            IActionResult result = await _controller.AddMultiple(products);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
            _mockService.Verify(s => s.AddRangeAsync(products, "Seller Name", "seller1"), Times.Once);
        }

        [Test]
        public async Task AddMultiple_NoJwt_ReturnsUnauthorized()
        {
            SetupControllerWithoutJwt();
            List<ProductDto> products = new() { new ProductDto { Name = "Item" } };

            IActionResult result = await _controller.AddMultiple(products);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task PlaceBid_WithValidJwt_SuccessfulBid_ReturnsCreated()
        {
            SetupControllerWithJwt("Bidder Name", "bidder1");
            BidDto bid = new("b-1", "p-1", "Bidder Name", "bidder1", 200, DateTime.UtcNow, true);
            ProductDto updatedProduct = new() { Id = "p-1", CurrentBid = 200 };

            _mockService.Setup(s => s.PlaceBidAsync("p-1", 200, "Bidder Name", "bidder1"))
                .ReturnsAsync((true, null, bid));
            _mockService.Setup(s => s.GetByIdAsync("p-1"))
                .ReturnsAsync(updatedProduct);

            IActionResult result = await _controller.PlaceBid("p-1", new ProductsController.BidRequest(200));

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task PlaceBid_WithValidJwt_ServiceRejectsLowBid_ReturnsBadRequest()
        {
            SetupControllerWithJwt("Bidder Name", "bidder1");
            _mockService.Setup(s => s.PlaceBidAsync("p-1", 5, "Bidder Name", "bidder1"))
                .ReturnsAsync((false, "Bid must be greater than current bid", (BidDto?)null));

            IActionResult result = await _controller.PlaceBid("p-1", new ProductsController.BidRequest(5));

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PlaceBid_NoJwt_ReturnsUnauthorized()
        {
            SetupControllerWithoutJwt();

            IActionResult result = await _controller.PlaceBid("p-1", new ProductsController.BidRequest(100));

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task GetMyReceivedFeedback_WithValidJwt_ReturnsOk()
        {
            SetupControllerWithJwt("Seller Name", "seller1");
            List<FeedbackItemDto> feedbacks = new()
            {
                new FeedbackItemDto("f-1", "p-1", "Camera", 5, "Great!", DateTime.UtcNow, "buyer1")
            };
            _mockService.Setup(s => s.GetFeedbackReceivedByUserAsync("Seller Name")).ReturnsAsync(feedbacks);

            IActionResult result = await _controller.GetMyReceivedFeedback();

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.EqualTo(feedbacks));
        }

        [Test]
        public async Task GetMyReceivedFeedback_NoJwt_ReturnsUnauthorized()
        {
            SetupControllerWithoutJwt();

            IActionResult result = await _controller.GetMyReceivedFeedback();

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }
    }
}