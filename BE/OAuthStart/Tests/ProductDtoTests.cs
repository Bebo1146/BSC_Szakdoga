using NUnit.Framework;
using ServicesHoster.Services;

namespace Tests
{
    [TestFixture]
    public class ProductDtoTests
    {
        [Test]
        public void IsActive_ActiveStatusWithinTimeWindow_ReturnsTrue()
        {
            ProductDto product = new()
            {
                Status = ProductStatus.Active,
                AuctionStartTime = DateTime.UtcNow.AddHours(-1),
                AuctionEndTime = DateTime.UtcNow.AddHours(1)
            };

            Assert.That(product.IsActive, Is.True);
        }

        [Test]
        public void IsActive_ActiveStatusButAuctionNotStarted_ReturnsFalse()
        {
            ProductDto product = new()
            {
                Status = ProductStatus.Active,
                AuctionStartTime = DateTime.UtcNow.AddHours(1),
                AuctionEndTime = DateTime.UtcNow.AddHours(2)
            };

            Assert.That(product.IsActive, Is.False);
        }

        [Test]
        public void IsActive_ActiveStatusButAuctionEnded_ReturnsFalse()
        {
            ProductDto product = new()
            {
                Status = ProductStatus.Active,
                AuctionStartTime = DateTime.UtcNow.AddHours(-2),
                AuctionEndTime = DateTime.UtcNow.AddHours(-1)
            };

            Assert.That(product.IsActive, Is.False);
        }

        [TestCase(ProductStatus.Sold)]
        [TestCase(ProductStatus.Expired)]
        [TestCase(ProductStatus.Cancelled)]
        [TestCase(ProductStatus.Rejected)]
        public void IsActive_TerminalStatus_ReturnsFalse(ProductStatus status)
        {
            ProductDto product = new()
            {
                Status = status,
                AuctionStartTime = DateTime.UtcNow.AddHours(-1),
                AuctionEndTime = DateTime.UtcNow.AddHours(1)
            };

            Assert.That(product.IsActive, Is.False);
        }

        [Test]
        public void HasEnded_AuctionEndTimePassed_ReturnsTrue()
        {
            ProductDto product = new()
            {
                Status = ProductStatus.Active,
                AuctionEndTime = DateTime.UtcNow.AddHours(-1)
            };

            Assert.That(product.HasEnded, Is.True);
        }

        [TestCase(ProductStatus.Sold)]
        [TestCase(ProductStatus.Expired)]
        [TestCase(ProductStatus.Rejected)]
        public void HasEnded_TerminalStatus_ReturnsTrueRegardlessOfTime(ProductStatus status)
        {
            ProductDto product = new()
            {
                Status = status,
                AuctionEndTime = DateTime.UtcNow.AddHours(5)
            };

            Assert.That(product.HasEnded, Is.True);
        }

        [Test]
        public void HasEnded_ActiveAndNotExpired_ReturnsFalse()
        {
            ProductDto product = new()
            {
                Status = ProductStatus.Active,
                AuctionEndTime = DateTime.UtcNow.AddHours(1)
            };

            Assert.That(product.HasEnded, Is.False);
        }

        [Test]
        public void TimeRemaining_AuctionNotEnded_ReturnsPositiveTimeSpan()
        {
            ProductDto product = new()
            {
                Status = ProductStatus.Active,
                AuctionStartTime = DateTime.UtcNow.AddHours(-1),
                AuctionEndTime = DateTime.UtcNow.AddHours(1)
            };

            Assert.That(product.TimeRemaining, Is.Not.Null);
            Assert.That(product.TimeRemaining!.Value.TotalSeconds, Is.GreaterThan(0));
        }

        [Test]
        public void TimeRemaining_AuctionEnded_ReturnsNull()
        {
            ProductDto product = new()
            {
                Status = ProductStatus.Expired,
                AuctionEndTime = DateTime.UtcNow.AddHours(-1)
            };

            Assert.That(product.TimeRemaining, Is.Null);
        }
    }
}