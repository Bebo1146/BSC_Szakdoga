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
                Bidders =
                [
                    new ProductBidderDto("user-123", "collector_joe"),
                    new ProductBidderDto("kicsi kuki", "fos")
                ],
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
                Status = ProductStatus.Expired,
                ImageUrl = "https://example.com/images/watch.jpg",
                StartingPrice = 200.00m,
                CurrentBid = 350.00m,
                ReservePrice = 400.00m,
                AuctionStartTime = DateTime.UtcNow.AddDays(-4),
                AuctionEndTime = DateTime.UtcNow.AddHours(-1),
                TotalBids = 7,
                HighestBidderId = "user-123",
                HighestBidderUsername = "collector_joe",
                Bidders =
                [
                    new ProductBidderDto("user-123", "collector_joe"),
                    new ProductBidderDto("kicsi kuki", "fos")
                ],
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
                SellerId = "kicsi kuki",
                SellerUsername = "fos",
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
                HighestBidderId = "kicsi kuki",
                HighestBidderUsername = "fos",
                Bidders =
                [
                    new ProductBidderDto("kicsi kuki", "fos"),
                    new ProductBidderDto("user-654", "art_fan")
                ],
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

        // In-memory bid storage and winning-bid tracking
        private static readonly ConcurrentDictionary<string, BidDto> _bids = new();
        private static readonly ConcurrentDictionary<string, string> _winningBidByProduct = new();

        public Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            return Task.FromResult(Products.AsEnumerable());
        }

        public Task<ProductDto?> GetByIdAsync(string id)
        {
            ProductDto? product = Products.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(product);
        }

        public Task AddRangeAsync(IEnumerable<ProductDto> products, string userName, string userPreferedName)
        {
            DateTime now = DateTime.UtcNow;
            foreach (ProductDto product in products)
            {
                // Create new product with user info using the required constructor
                ProductDto productWithUser = new ProductDto
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
                    TotalBids = 0,
                    HighestBidderId = null,
                    HighestBidderUsername = null,
                    Bidders = [],
                    SellerId = userName,
                    SellerUsername = userPreferedName,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsCompleted = false,
                    TransactionStatus = null,
                    Feedback = null
                };
                Products.Add(productWithUser);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<ProductDto>> GetByUserAsync(string userId)
        {
            IEnumerable<ProductDto> userProducts = Products.Where(p => p.SellerId == userId);
            return Task.FromResult(userProducts.AsEnumerable());
        }

        public Task<(bool Success, string? Error, BidDto? Bid)> PlaceBidAsync(string productId, decimal amount, string bidderId, string bidderUsername)
        {
            ProductDto? product = Products.FirstOrDefault(p => p.Id == productId);
            if (product is null)
            {
                return Task.FromResult<(bool, string?, BidDto?)>((false, "Product not found", null));
            }

            //if (!product.IsActive)
            //{
            //    return Task.FromResult<(bool, string?, BidDto?)>((false, "Auction is not active", null));
            //}

            if (product.SellerId == bidderId)
            {
                return Task.FromResult<(bool, string?, BidDto?)>((false, "Seller cannot place bids on their own product", null));
            }

            decimal currentThreshold = (product.CurrentBid.HasValue && product.CurrentBid.Value > 0m) ? product.CurrentBid.Value : product.StartingPrice;
            if (amount <= currentThreshold)
            {
                return Task.FromResult<(bool, string?, BidDto?)>((false, $"Bid must be greater than current bid ({currentThreshold:C})", null));
            }

            DateTime now = DateTime.UtcNow;
            string bidId = $"b-{Guid.NewGuid():N}";

            // Update previous winning bid if exists
            if (_winningBidByProduct.TryGetValue(productId, out string? previousWinningId) && !string.IsNullOrEmpty(previousWinningId))
            {
                if (_bids.TryGetValue(previousWinningId, out BidDto? previousBid) && previousBid is not null && previousBid.IsWinningBid)
                {
                    // mark previous as not winning
                    _bids[previousWinningId] = previousBid with { IsWinningBid = false };
                }
            }

            // Create and store the new winning bid
            BidDto newBid = new(bidId, productId, bidderId, bidderUsername, amount, now, true);
            _bids[bidId] = newBid;
            _winningBidByProduct[productId] = bidId;

            // Update product state
            product.CurrentBid = amount;
            product.TotalBids += 1;
            product.HighestBidderId = bidderId;
            product.HighestBidderUsername = bidderUsername;
            product.UpdatedAt = now;

            if (!product.Bidders.Any(b => b.BidderId == bidderId))
            {
                product.Bidders.Add(new ProductBidderDto(bidderId, bidderUsername));
            }

            return Task.FromResult<(bool, string?, BidDto?)>((true, null, newBid));
        }

        public Task<IEnumerable<BidDto>> GetBidsAsync(string productId)
        {
            IEnumerable<BidDto> bids = _bids.Values
                .Where(b => b.ProductId == productId)
                .OrderBy(b => b.BidTime)
                .AsEnumerable();

            return Task.FromResult(bids);
        }

        public Task<IEnumerable<ProductDto>> GetProductsByBidderAsync(string bidderId)
        {
            IEnumerable<ProductDto> productsBidder = Products
                .Where(p => p.Bidders.Any(b => b.BidderId == bidderId));

            return Task.FromResult(productsBidder);
        }

        public Task<(bool Success, string? Error, ProductDto? Product)> MarkAsSoldAsync(string id)
        {
            ProductDto? product = Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
            {
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Product not found", null));
            }

            product.Status = ProductStatus.Sold;
            product.IsCompleted = true;
            product.TransactionStatus = TransactionStatus.Completed;
            product.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<(bool, string?, ProductDto?)>((true, null, product));
        }
    }
}