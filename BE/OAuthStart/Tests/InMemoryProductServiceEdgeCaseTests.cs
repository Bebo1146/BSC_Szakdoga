using NUnit.Framework;
using ServicesHoster.Services;

namespace Tests
{
    [TestFixture]
    public class InMemoryProductServiceEdgeCaseTests
    {
        private InMemoryProductService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new InMemoryProductService();
        }

        [Test]
        public async Task GetActiveProductsAsync_ReturnsOnlyActiveProducts()
        {
            IEnumerable<ProductDto> active = await _service.GetActiveProductsAsync();

            Assert.That(active.All(p => p.Status == ProductStatus.Active), Is.True);
        }

        [Test]
        public async Task GetActiveProductsAsync_DoesNotIncludeExpiredOrSold()
        {
            IEnumerable<ProductDto> active = await _service.GetActiveProductsAsync();

            Assert.That(active.Any(p => p.Status == ProductStatus.Expired), Is.False);
            Assert.That(active.Any(p => p.Status == ProductStatus.Sold), Is.False);
        }

        [Test]
        public async Task PlaceBidAsync_AddsBidderToListOnce()
        {
            await _service.PlaceBidAsync("p-1003", 200, "repeat-bidder", "Repeat");
            await _service.PlaceBidAsync("p-1003", 300, "repeat-bidder", "Repeat");

            ProductDto? product = await _service.GetByIdAsync("p-1003");
            int bidderCount = product!.Bidders.Count(b => b.BidderId == "repeat-bidder");

            Assert.That(bidderCount, Is.EqualTo(1));
        }

        [Test]
        public async Task GetBidsAsync_ReturnsEmptyForProductWithNoBids()
        {
            IEnumerable<BidDto> bids = await _service.GetBidsAsync("p-1008");

            Assert.That(bids, Is.Empty);
        }

        [Test]
        public async Task GetBidsAsync_ReturnsBidsInChronologicalOrder()
        {
            await _service.PlaceBidAsync("p-1007", 500, "a", "A");
            await Task.Delay(10);
            await _service.PlaceBidAsync("p-1007", 600, "b", "B");

            List<BidDto> bids = (await _service.GetBidsAsync("p-1007")).ToList();

            Assert.That(bids.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(bids[0].BidTime, Is.LessThanOrEqualTo(bids[1].BidTime));
        }

        [Test]
        public async Task AddRangeAsync_SetsIdForEachProduct()
        {
            List<ProductDto> newProducts = new()
            {
                new ProductDto { Name = "A", StartingPrice = 10, AuctionStartTime = DateTime.UtcNow, AuctionEndTime = DateTime.UtcNow.AddHours(1) },
                new ProductDto { Name = "B", StartingPrice = 20, AuctionStartTime = DateTime.UtcNow, AuctionEndTime = DateTime.UtcNow.AddHours(1) }
            };

            await _service.AddRangeAsync(newProducts, "seller", "Seller");

            IEnumerable<ProductDto> all = await _service.GetAllAsync();
            ProductDto? a = all.FirstOrDefault(p => p.Name == "A");
            ProductDto? b = all.FirstOrDefault(p => p.Name == "B");

            Assert.That(a!.Id, Is.Not.EqualTo(b!.Id));
            Assert.That(a.Id, Does.StartWith("p-"));
            Assert.That(b.Id, Does.StartWith("p-"));
        }

        [Test]
        public async Task GetProductsByBidderAsync_ExcludesRejectedProducts()
        {
            await _service.PlaceBidAsync("p-1003", 200, "test-bidder", "TestBidder");
            await _service.MarkAsRejectedAsync("p-1003", "spam");

            IEnumerable<ProductDto> products = await _service.GetProductsByBidderAsync("test-bidder");

            Assert.That(products.Any(p => p.Id == "p-1003"), Is.False);
        }

        [Test]
        public async Task GetProductsByBidderAsync_ExcludesExpiredWhereNotHighestBidder()
        {
            IEnumerable<ProductDto> products = await _service.GetProductsByBidderAsync("bob");

            Assert.That(products.Any(p => p.Id == "p-1002"), Is.False);
        }

        [Test]
        public async Task GetFeedbackReceivedByUserAsync_ReturnsOnlyFeedbackedSoldOrRejected()
        {
            await _service.MarkAsSoldAsync("p-1003");
            await _service.AddFeedbackAsync("p-1003", new FeedbackDto(4, "Good"));

            IEnumerable<FeedbackItemDto> feedback = await _service.GetFeedbackReceivedByUserAsync("Boti Boti");

            Assert.That(feedback.Any(f => f.ProductId == "p-1003"), Is.True);
            Assert.That(feedback.All(f => f.Rating >= 1 && f.Rating <= 5), Is.True);
        }

        [Test]
        public async Task ExpireEndedAuctionsAsync_DoesNotAffectDraftOrSoldProducts()
        {
            ProductDto? draft = await _service.GetByIdAsync("p-1008");
            ProductDto? sold = await _service.GetByIdAsync("p-1004");

            ProductStatus draftBefore = draft!.Status;
            ProductStatus soldBefore = sold!.Status;

            await _service.ExpireEndedAuctionsAsync();

            Assert.That(draft.Status, Is.EqualTo(draftBefore));
            Assert.That(sold.Status, Is.EqualTo(soldBefore));
        }

        [Test]
        public async Task MarkAsAcceptedAsync_NonExistentProduct_Fails()
        {
            (bool success, string? error, _) = await _service.MarkAsAcceptedAsync("no-such-id");

            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo("Product not found"));
        }

        [Test]
        public async Task MarkAsRejectedAsync_NonExistentProduct_Fails()
        {
            (bool success, string? error, _) = await _service.MarkAsRejectedAsync("no-such-id", "reason");

            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo("Product not found"));
        }

        [Test]
        public async Task AddFeedbackAsync_NonExistentProduct_Fails()
        {
            (bool success, string? error, _) = await _service.AddFeedbackAsync("no-such-id", new FeedbackDto(3, "ok"));

            Assert.That(success, Is.False);
            Assert.That(error, Is.EqualTo("Product not found"));
        }
    }
}