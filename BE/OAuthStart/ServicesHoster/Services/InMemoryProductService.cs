using System.Collections.Concurrent;

namespace ServicesHoster.Services
{
    public class InMemoryProductService : IProductService
    {
        private static readonly ConcurrentBag<ProductDto> Products = new(new[]
        {
            new ProductDto
            {
                Id = "p-1001",
                Name = "Vintage Camera",
                Description = "Rare vintage camera from the 1960s in excellent condition",
                Category = "Electronics",
                Status = ProductStatus.Active,
                ImageUrl = "https://example.com/images/camera.jpg",
                StartingPrice = 50.00m,
                CurrentBid = 75.00m,
                ReservePrice = 100.00m,
                AuctionStartTime = DateTime.UtcNow.AddDays(-2),
                AuctionEndTime = DateTime.UtcNow.AddDays(5),
                TotalBids = 3,
                HighestBidderId = "user-123",
                HighestBidderUsername = "collector_joe",
                SellerId = "system",
                SellerUsername = "admin",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-1),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null
            },
            new ProductDto
            {
                Id = "p-1002",
                Name = "Antique Watch",
                Description = "Swiss made pocket watch, gold plated",
                Category = "Jewelry",
                Status = ProductStatus.Active,
                ImageUrl = "https://example.com/images/watch.jpg",
                StartingPrice = 200.00m,
                CurrentBid = 350.00m,
                ReservePrice = 400.00m,
                AuctionStartTime = DateTime.UtcNow.AddDays(-1),
                AuctionEndTime = DateTime.UtcNow.AddDays(3),
                TotalBids = 7,
                HighestBidderId = "user-456",
                HighestBidderUsername = "watch_enthusiast",
                SellerId = "system",
                SellerUsername = "admin",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null
            },
            new ProductDto
            {
                Id = "p-1003",
                Name = "Leather Handbag",
                Description = "Designer leather handbag, limited edition",
                Category = "Fashion",
                Status = ProductStatus.Active,
                ImageUrl = "https://example.com/images/handbag.jpg",
                StartingPrice = 100.00m,
                CurrentBid = 150.00m,
                ReservePrice = 180.00m,
                AuctionStartTime = DateTime.UtcNow.AddHours(-12),
                AuctionEndTime = DateTime.UtcNow.AddDays(2),
                TotalBids = 5,
                HighestBidderId = "user-789",
                HighestBidderUsername = "fashionista",
                SellerId = "system",
                SellerUsername = "admin",
                CreatedAt = DateTime.UtcNow.AddHours(-12),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-15),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null
            },
            new ProductDto
            {
                Id = "p-1004",
                Name = "Vintage Vinyl Records",
                Description = "Collection of 20 vinyl records from the 70s",
                Category = "Music",
                Status = ProductStatus.Sold,
                ImageUrl = "https://example.com/images/vinyl.jpg",
                StartingPrice = 30.00m,
                CurrentBid = 120.00m,
                ReservePrice = 80.00m,
                AuctionStartTime = DateTime.UtcNow.AddDays(-10),
                AuctionEndTime = DateTime.UtcNow.AddDays(-3),
                TotalBids = 12,
                HighestBidderId = "user-321",
                HighestBidderUsername = "music_lover",
                SellerId = "system",
                SellerUsername = "admin",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                IsCompleted = true,
                TransactionStatus = TransactionStatus.Completed,
            },
            new ProductDto
            {
                Id = "p-1005",
                Name = "Gaming Console",
                Description = "Latest generation gaming console, barely used",
                Category = "Electronics",
                Status = ProductStatus.Draft,
                ImageUrl = "https://example.com/images/console.jpg",
                StartingPrice = 250.00m,
                CurrentBid = 250,
                ReservePrice = 350.00m,
                AuctionStartTime = DateTime.UtcNow.AddDays(1),
                AuctionEndTime = DateTime.UtcNow.AddDays(8),
                TotalBids = 0,
                HighestBidderId = null,
                HighestBidderUsername = null,
                SellerId = "system",
                SellerUsername = "admin",
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                UpdatedAt = DateTime.MaxValue,
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null
            },
            new ProductDto
            {
                Id = "p-1006",
                Name = "Oil Painting",
                Description = "Original oil painting by local artist",
                Category = "Art",
                Status = ProductStatus.Expired,
                ImageUrl = "https://example.com/images/painting.jpg",
                StartingPrice = 500.00m,
                CurrentBid = 450.00m,
                ReservePrice = 600.00m,
                AuctionStartTime = DateTime.UtcNow.AddDays(-7),
                AuctionEndTime = DateTime.UtcNow.AddDays(-1),
                TotalBids = 4,
                HighestBidderId = "user-555",
                HighestBidderUsername = "art_collector",
                SellerId = "system",
                SellerUsername = "admin",
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null
            },
            new ProductDto
            {
                Id = "p-1007",
                Name = "Mountain Bike",
                Description = "Professional mountain bike with full suspension",
                Category = "Sports",
                Status = ProductStatus.Active,
                ImageUrl = "https://example.com/images/bike.jpg",
                StartingPrice = 300.00m,
                CurrentBid = 420.00m,
                ReservePrice = 500.00m,
                AuctionStartTime = DateTime.UtcNow.AddHours(-6),
                AuctionEndTime = DateTime.UtcNow.AddDays(4),
                TotalBids = 6,
                HighestBidderId = "user-888",
                HighestBidderUsername = "mountain_biker",
                SellerId = "system",
                SellerUsername = "admin",
                CreatedAt = DateTime.UtcNow.AddHours(-6),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-45),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null
            }
        });

        public Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            return Task.FromResult(Products.AsEnumerable());
        }

        public Task<ProductDto?> GetByIdAsync(string id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(product);
        }

        public Task AddRangeAsync(IEnumerable<ProductDto> products, string userId)
        {
            var now = DateTime.UtcNow;
            foreach (var product in products)
            {
                // Create new product with user info using the required constructor
                var productWithUser = new ProductDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Category = product.Category,
                    Status = product.Status,
                    ImageUrl = product.ImageUrl,
                    StartingPrice = product.StartingPrice,
                    CurrentBid = product.CurrentBid ?? 0m,
                    ReservePrice = product.ReservePrice ?? 0m,
                    AuctionStartTime = product.AuctionStartTime,
                    AuctionEndTime = product.AuctionEndTime,
                    TotalBids = product.TotalBids,
                    HighestBidderId = product.HighestBidderId,
                    HighestBidderUsername = product.HighestBidderUsername,
                    SellerId = userId,
                    SellerUsername = product.SellerUsername,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsCompleted = product.IsCompleted,
                    TransactionStatus = product.TransactionStatus,
                    Feedback = product.Feedback
                };
                Products.Add(productWithUser);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<ProductDto>> GetByUserAsync(string userId)
        {
            var userProducts = Products.Where(p => p.SellerId == userId);
            return Task.FromResult(userProducts.AsEnumerable());
        }
    }
}