using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using ServicesHoster.Controllers;
using ServicesHoster.Services;

namespace Tests
{
    [TestFixture]
    public class ProductsControllerTests
    {
        private Mock<IProductService> _mockService = null!;
        private ProductsController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _mockService = new Mock<IProductService>();
            _controller = new ProductsController(_mockService.Object);
        }

        [Test]
        public async Task GetAll_ReturnsOkWithProducts()
        {
            List<ProductDto> products = new()
            {
                new ProductDto { Id = "p-1", Name = "Item A" },
                new ProductDto { Id = "p-2", Name = "Item B" }
            };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(products);

            IActionResult result = await _controller.GetAll();

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.StatusCode, Is.EqualTo(200));
            Assert.That(ok.Value, Is.EqualTo(products));
        }

        [Test]
        public async Task GetAll_EmptyList_ReturnsOk()
        {
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(Enumerable.Empty<ProductDto>());

            IActionResult result = await _controller.GetAll();

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
        }

        [Test]
        public async Task GetById_ExistingProduct_ReturnsOk()
        {
            ProductDto product = new() { Id = "p-1", Name = "Camera" };
            _mockService.Setup(s => s.GetByIdAsync("p-1")).ReturnsAsync(product);

            IActionResult result = await _controller.GetById("p-1");

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.EqualTo(product));
        }

        [Test]
        public async Task GetById_NonExistent_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetByIdAsync("missing")).ReturnsAsync((ProductDto?)null);

            IActionResult result = await _controller.GetById("missing");

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task AddMultiple_NullBody_ReturnsBadRequest()
        {
            IActionResult result = await _controller.AddMultiple(null);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AddMultiple_EmptyList_ReturnsBadRequest()
        {
            IActionResult result = await _controller.AddMultiple(new List<ProductDto>());

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PlaceBid_NullRequest_ReturnsBadRequest()
        {
            IActionResult result = await _controller.PlaceBid("p-1", null);

            BadRequestObjectResult bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            Assert.That(bad!.Value!.ToString(), Does.Contain("Invalid bid request"));
        }

        [Test]
        public async Task PlaceBid_ZeroAmount_ReturnsBadRequest()
        {
            IActionResult result = await _controller.PlaceBid("p-1", new ProductsController.BidRequest(0));

            BadRequestObjectResult bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            Assert.That(bad!.Value!.ToString(), Does.Contain("greater than zero"));
        }

        [Test]
        public async Task PlaceBid_NegativeAmount_ReturnsBadRequest()
        {
            IActionResult result = await _controller.PlaceBid("p-1", new ProductsController.BidRequest(-5));

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MarkAsSold_NullIds_ReturnsBadRequest()
        {
            IActionResult result = await _controller.MarkAsSold(null);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MarkAsSold_EmptyIds_ReturnsBadRequest()
        {
            IActionResult result = await _controller.MarkAsSold(new List<string>());

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MarkAsSold_ValidIds_CallsServiceAndReturnsOk()
        {
            ProductDto soldProduct = new() { Id = "p-1", Status = ProductStatus.Sold };
            _mockService.Setup(s => s.MarkAsSoldAsync("p-1"))
                .ReturnsAsync((true, null, soldProduct));

            IActionResult result = await _controller.MarkAsSold(new List<string> { "p-1" });

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            _mockService.Verify(s => s.MarkAsSoldAsync("p-1"), Times.Once);
        }

        [Test]
        public async Task MarkAsSold_MixedResults_ReturnsUpdatedAndFailed()
        {
            _mockService.Setup(s => s.MarkAsSoldAsync("p-1"))
                .ReturnsAsync((true, null, new ProductDto { Id = "p-1" }));
            _mockService.Setup(s => s.MarkAsSoldAsync("p-missing"))
                .ReturnsAsync((false, "Product not found", (ProductDto?)null));

            IActionResult result = await _controller.MarkAsSold(new List<string> { "p-1", "p-missing" });

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
        }

        [Test]
        public async Task MarkAsSold_SkipsWhitespaceIds()
        {
            _mockService.Setup(s => s.MarkAsSoldAsync(It.IsAny<string>()))
                .ReturnsAsync((true, null, new ProductDto()));

            await _controller.MarkAsSold(new List<string> { "  ", "", "p-1" });

            _mockService.Verify(s => s.MarkAsSoldAsync("p-1"), Times.Once);
            _mockService.Verify(s => s.MarkAsSoldAsync("  "), Times.Never);
            _mockService.Verify(s => s.MarkAsSoldAsync(""), Times.Never);
        }

        [Test]
        public async Task MarkAsRejected_NullRequests_ReturnsBadRequest()
        {
            IActionResult result = await _controller.MarkAsRejected(null);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MarkAsRejected_EmptyRequests_ReturnsBadRequest()
        {
            IActionResult result = await _controller.MarkAsRejected(new List<RejectProductRequest>());

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MarkAsRejected_ValidRequest_CallsServiceWithReason()
        {
            ProductDto rejected = new() { Id = "p-1", Status = ProductStatus.Rejected };
            _mockService.Setup(s => s.MarkAsRejectedAsync("p-1", "Policy violation"))
                .ReturnsAsync((true, null, rejected));

            List<RejectProductRequest> requests = new()
            {
                new RejectProductRequest("p-1", "Policy violation")
            };

            IActionResult result = await _controller.MarkAsRejected(requests);

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            _mockService.Verify(s => s.MarkAsRejectedAsync("p-1", "Policy violation"), Times.Once);
        }

        [Test]
        public async Task MarkAsAccepted_NullIds_ReturnsBadRequest()
        {
            IActionResult result = await _controller.MarkAsAccepted(null);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task MarkAsAccepted_ValidIds_CallsService()
        {
            ProductDto accepted = new() { Id = "p-1", Status = ProductStatus.Active };
            _mockService.Setup(s => s.MarkAsAcceptedAsync("p-1"))
                .ReturnsAsync((true, null, accepted));

            IActionResult result = await _controller.MarkAsAccepted(new List<string> { "p-1" });

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            _mockService.Verify(s => s.MarkAsAcceptedAsync("p-1"), Times.Once);
        }

        [Test]
        public async Task AddFeedback_NullBody_ReturnsBadRequest()
        {
            IActionResult result = await _controller.AddFeedback("p-1", null);

            BadRequestObjectResult bad = result as BadRequestObjectResult;
            Assert.That(bad, Is.Not.Null);
            Assert.That(bad!.Value!.ToString(), Does.Contain("Invalid feedback"));
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(6)]
        [TestCase(10)]
        public async Task AddFeedback_InvalidRating_ReturnsBadRequest(int rating)
        {
            IActionResult result = await _controller.AddFeedback("p-1", new FeedbackDto(rating, "text"));

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AddFeedback_NullRating_ReturnsBadRequest()
        {
            IActionResult result = await _controller.AddFeedback("p-1", new FeedbackDto(null, "text"));

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task AddFeedback_ValidFeedback_ReturnsOk()
        {
            FeedbackDto feedback = new(4, "Nice item");
            ProductDto product = new() { Id = "p-1", Feedback = feedback };
            _mockService.Setup(s => s.AddFeedbackAsync("p-1", feedback))
                .ReturnsAsync((true, null, product));

            IActionResult result = await _controller.AddFeedback("p-1", feedback);

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.EqualTo(product));
        }

        [Test]
        public async Task AddFeedback_ServiceReturnsFailure_ReturnsBadRequest()
        {
            FeedbackDto feedback = new(3, "text");
            _mockService.Setup(s => s.AddFeedbackAsync("p-1", feedback))
                .ReturnsAsync((false, "Feedback can only be added to sold products", (ProductDto?)null));

            IActionResult result = await _controller.AddFeedback("p-1", feedback);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public void Health_ReturnsOk()
        {
            IActionResult result = _controller.Health();

            OkObjectResult ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(ok!.Value, Is.EqualTo("OK"));
        }
    }
}