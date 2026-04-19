using NUnit.Framework;
using ServicesHoster.Services;

namespace Tests
{
    [TestFixture]
    public class InMemoryProductServiceTests
    {
        private InMemoryProductService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new InMemoryProductService();
        }

        [Test]
        public async Task GetAllAsync_ReturnsSeededProducts()
        {
            IEnumerable<ProductDto> products = await _service.GetAllAsync();

            Assert.That(products, Is.Not.Empty);
        }

        [Test]
        public async Task GetByIdAsync_ExistingId_ReturnsProduct()
        {
            ProductDto? product = await _service.GetByIdAsync("p-1000");

            Assert.That(product, Is.Not.Null);
            Assert.That(product!.Name, Is.EqualTo("Classic Film Camera"));
        }

        [Test]
        public async Task GetByIdAsync_NonExistingId_ReturnsNull()
        {
            ProductDto? product = await _service.GetByIdAsync("non-existent-id");

            Assert.That(product, Is.Null);
        }

        [Test]
        public async Task AddRangeAsync_AddsProductsWithCorrectSellerInfo()
        {
            List<ProductDto> newProducts = new()
            {
                new ProductDto
                {
                    Name = "Test Product",
                    Description = "Test Description",
                    Category = "Test",
                    Status = ProductStatus.Draft,
                    StartingPrice = 100,
                    AuctionStartTime = DateTime.UtcNow,
                    AuctionEndTime = DateTime.UtcNow.AddHours(1)
                }
            };

            await _service.AddRangeAsync(newProducts, "seller-id", "Seller Name");

            IEnumerable<ProductDto> allProducts = await _service.GetAllAsync();
            ProductDto? added = allProducts.FirstOrDefault(p => p.Name == "Test Product");

            Assert.That(added, Is.Not.Null);
            Assert.That(added!.SellerId, Is.EqualTo("seller-id"));
            Assert.That(added.SellerUsername, Is.EqualTo("Seller Name"));
            Assert.That(added.Id, Does.StartWith("p-"));
            Assert.That(added.CurrentBid, Is.EqualTo(100));
            Assert.That(added.TotalBids, Is.EqualTo(0));
        }

        [Test]
        public async Task GetByUserAsync_ReturnsOnlyUserProducts()
        {
            IEnumerable<ProductDto> products = await _service.GetByUserAsync("Beni Beni");

            Assert.That(products, Is.Not.Empty);
            Assert.That(products.All(p => p.SellerId == "Beni Beni"), Is.True);
        }

        [Test]
        public async Task GetByUserAsync_ExcludesRejectedProducts()
        {
            await _service.MarkAsRejectedAsync("p-1000", "test reason");

            IEnumerable<ProductDto> products = await _service.GetByUserAsync("Beni Beni");

            Assert.That(products.Any(p => p.Id == "p-1000"), Is.False);
        }

        [Test]
        public async Task PlaceBidAsync_ValidBid_Succeeds()
        {
            (bool success, string? error, BidDto? bid) = await _service.PlaceBidAsync("p-1003", 200, "bidder-1", "Bidder");

            Assert.That(success, Is.True);
            Assert.That(error, Is.Null);
            Assert.That(bid, Is.Not.Null);
            Assert.That(bid!.Amount, Is.EqualTo(200));
            Assert.That(bid.IsWinningBid, Is.True);
        }

        [Test]
        public async Task PlaceBidAsync_BidTooLow_Fails()
        {
            (bool success, string? error, BidDto? bid) = await _service.PlaceBidAsync("p-1003", 10, "bidder-1", "Bidder");

            Assert.That(success, Is.False);
            Assert.That(error, Does.Contain("greater than current bid"));
            Assert.That(bid, Is.Null);
        }

        [Test]
        public async Task PlaceBidAsync_SellerBidsOnOwnProduct_Fails()
        {
            (bool success, string? error, BidDto? bid) = await _service.PlaceBidAsync("p-1003", 500, "Boti Boti", "Boti");

            Assert.That(success, Is.False);
            Assert.That(error, Does.Contain("Seller cannot place bids"));
            Assert.That(bid, Is.Null);
        }

        [Test]
        public async Task PlaceBidAsync_NonExistentProduct_Fails()
        {
            (bool success, string? error, BidDto? bid) = await _service.PlaceBidAsync("no-such-product", 100, "bidder-1", "Bidder");

            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo("Product not found"));
        }

        [Test]
        public async Task PlaceBidAsync_UpdatesProductBidInfo()
        {
            await _service.PlaceBidAsync("p-1003", 200, "new-bidder", "NewBidder");

            ProductDto? product = await _service.GetByIdAsync("p-1003");

            Assert.That(product!.CurrentBid, Is.EqualTo(200));
            Assert.That(product.HighestBidderId, Is.EqualTo("new-bidder"));
            Assert.That(product.HighestBidderUsername, Is.EqualTo("NewBidder"));
            Assert.That(product.Bidders.Any(b => b.BidderId == "new-bidder"), Is.True);
        }

        [Test]
        public async Task PlaceBidAsync_PreviousWinningBidIsUnset()
        {
            await _service.PlaceBidAsync("p-1007", 500, "bidder-A", "A");
            await _service.PlaceBidAsync("p-1007", 600, "bidder-B", "B");

            IEnumerable<BidDto> bids = await _service.GetBidsAsync("p-1007");
            BidDto? firstBid = bids.FirstOrDefault(b => b.BidderId == "bidder-A");
            BidDto? secondBid = bids.FirstOrDefault(b => b.BidderId == "bidder-B");

            Assert.That(firstBid!.IsWinningBid, Is.False);
            Assert.That(secondBid!.IsWinningBid, Is.True);
        }

        [Test]
        public async Task MarkAsSoldAsync_ExistingProduct_SetsSoldStatus()
        {
            (bool success, string? error, ProductDto? product) = await _service.MarkAsSoldAsync("p-1003");

            Assert.That(success, Is.True);
            Assert.That(product!.Status, Is.EqualTo(ProductStatus.Sold));
            Assert.That(product.IsCompleted, Is.True);
            Assert.That(product.TransactionStatus, Is.EqualTo(TransactionStatus.Completed));
        }

        [Test]
        public async Task MarkAsSoldAsync_NonExistentProduct_Fails()
        {
            (bool success, string? error, ProductDto? product) = await _service.MarkAsSoldAsync("no-such-id");

            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo("Product not found"));
        }

        [Test]
        public async Task MarkAsRejectedAsync_SetsRejectedStatusAndFeedback()
        {
            (bool success, _, ProductDto? product) = await _service.MarkAsRejectedAsync("p-1003", "Violates policy");

            Assert.That(success, Is.True);
            Assert.That(product!.Status, Is.EqualTo(ProductStatus.Rejected));
            Assert.That(product.Feedback, Is.Not.Null);
            Assert.That(product.Feedback!.Comment, Is.EqualTo("Violates policy"));
            Assert.That(product.Feedback.Rating, Is.Null);
        }

        [Test]
        public async Task MarkAsAcceptedAsync_SetsActiveStatusAndResetsCurrentBid()
        {
            (bool success, _, ProductDto? product) = await _service.MarkAsAcceptedAsync("p-1008");

            Assert.That(success, Is.True);
            Assert.That(product!.Status, Is.EqualTo(ProductStatus.Active));
            Assert.That(product.CurrentBid, Is.EqualTo(product.StartingPrice));
        }

        [Test]
        public async Task AddFeedbackAsync_SoldProduct_Succeeds()
        {
            await _service.MarkAsSoldAsync("p-1003");

            FeedbackDto feedback = new(5, "Excellent!");
            (bool success, _, ProductDto? product) = await _service.AddFeedbackAsync("p-1003", feedback);

            Assert.That(success, Is.True);
            Assert.That(product!.Feedback!.Rating, Is.EqualTo(5));
            Assert.That(product.Feedback.Comment, Is.EqualTo("Excellent!"));
        }

        [Test]
        public async Task AddFeedbackAsync_NotSoldProduct_Fails()
        {
            FeedbackDto feedback = new(5, "Great");
            (bool success, string? error, _) = await _service.AddFeedbackAsync("p-1003", feedback);

            Assert.That(success, Is.False);
            Assert.That(error, Does.Contain("Feedback can only be added to sold products"));
        }

        [Test]
        public async Task AddFeedbackAsync_InvalidRating_Fails()
        {
            await _service.MarkAsSoldAsync("p-1007");

            FeedbackDto feedback = new(6, "Too high");
            (bool success, string? error, _) = await _service.AddFeedbackAsync("p-1007", feedback);

            Assert.That(success, Is.False);
            Assert.That(error, Does.Contain("Rating must be between 1 and 5"));
        }

        [Test]
        public async Task ExpireEndedAuctionsAsync_ExpiresOverdueActiveProducts()
        {
            List<ProductDto> products = new()
            {
                new ProductDto
                {
                    Name = "Expiring Item",
                    Status = ProductStatus.Active,
                    StartingPrice = 10,
                    AuctionStartTime = DateTime.UtcNow.AddHours(-2),
                    AuctionEndTime = DateTime.UtcNow.AddSeconds(-1)
                }
            };
            await _service.AddRangeAsync(products, "seller", "Seller");

            await _service.ExpireEndedAuctionsAsync();

            IEnumerable<ProductDto> all = await _service.GetAllAsync();
            ProductDto? expired = all.FirstOrDefault(p => p.Name == "Expiring Item");

            Assert.That(expired!.Status, Is.EqualTo(ProductStatus.Expired));
        }
    }
}