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
                StartingPrice = 50,
                CurrentBid = 75,
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
                StartingPrice = 200,
                CurrentBid = 350,
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
                StartingPrice = 100,
                CurrentBid = 150,
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
                StartingPrice = 30,
                CurrentBid = 120,
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
                TransactionStatus = TransactionStatus.Completed
            },
            new ProductDto
            {
                Id = "p-1005",
                Name = "Gaming Console",
                Description = "Latest generation gaming console, barely used",
                Category = "Electronics",
                Status = ProductStatus.Sold,
                ImageUrl = "https://example.com/images/console.jpg",
                StartingPrice = 250,
                CurrentBid = 250,
                AuctionStartTime = DateTime.UtcNow.AddDays(-8),
                AuctionEndTime = DateTime.UtcNow.AddDays(-1),
                TotalBids = 0,
                HighestBidderId = null,
                HighestBidderUsername = null,
                SellerId = "kicsi kuki",
                SellerUsername = "fos",
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                UpdatedAt = DateTime.UtcNow,
                IsCompleted = true,
                TransactionStatus = TransactionStatus.Completed,
                Feedback = new FeedbackDto(
                    5,
                    "Very smooth transaction, everything was exactly as described.")
            },
            new ProductDto
            {
                Id = "p-1006",
                Name = "Oil Painting",
                Description = "Original oil painting by local artist",
                Category = "Art",
                Status = ProductStatus.Expired,
                ImageUrl = "https://example.com/images/painting.jpg",
                StartingPrice = 500,
                CurrentBid = 450,
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
                StartingPrice = 300,
                CurrentBid = 420,
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
            },
            new ProductDto
            {
                Id = "p-1008",
                Name = "Wireless Headphones",
                Description = "Noise-cancelling wireless headphones in excellent condition",
                Category = "Electronics",
                Status = ProductStatus.Draft,
                ImageUrl = "https://example.com/images/headphones.jpg",
                StartingPrice = 180,
                CurrentBid = 180,
                AuctionStartTime = DateTime.UtcNow.AddDays(2),
                AuctionEndTime = DateTime.UtcNow.AddDays(9),
                TotalBids = 0,
                HighestBidderId = null,
                HighestBidderUsername = null,
                SellerId = "kicsi kuki",
                SellerUsername = "fos",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-1),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null
            }
        });

        private static readonly ConcurrentDictionary<string, BidDto> _bids = new();
        private static readonly ConcurrentDictionary<string, string> _winningBidByProduct = new();

        private static void ExpireEndedAuctions()
        {
            DateTime now = DateTime.UtcNow;
            foreach (ProductDto product in Products)
            {
                if (product.Status == ProductStatus.Active && now >= product.AuctionEndTime)
                {
                    product.Status = ProductStatus.Expired;
                    product.UpdatedAt = now;
                }
            }
        }

        // Returns all products unfiltered (used by controller/admin logic)
        public Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            return Task.FromResult(Products.AsEnumerable());
        }

        // Returns only active products, auto-expires ended ones (used by SignalR timer)
        public Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
        {
            ExpireEndedAuctions();
            return Task.FromResult(Products.Where(p => p.Status == ProductStatus.Active).AsEnumerable());
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
                ProductDto productWithUser = new ProductDto
                {
                    Id = $"p-{Guid.NewGuid():N}",
                    Name = product.Name,
                    Description = product.Description,
                    Category = product.Category,
                    Status = product.Status,
                    ImageUrl = product.ImageUrl,
                    StartingPrice = product.StartingPrice,
                    CurrentBid = product.StartingPrice,
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
            IEnumerable<ProductDto> userProducts = Products
                .Where(p => p.SellerId == userId && p.Status != ProductStatus.Rejected);

            return Task.FromResult(userProducts.AsEnumerable());
        }

        public Task<(bool Success, string? Error, BidDto? Bid)> PlaceBidAsync(string productId, int amount, string bidderId, string bidderUsername)
        {
            ProductDto? product = Products.FirstOrDefault(p => p.Id == productId);
            if (product is null)
            {
                return Task.FromResult<(bool, string?, BidDto?)>((false, "Product not found", null));
            }

            if (product.SellerId == bidderId)
            {
                return Task.FromResult<(bool, string?, BidDto?)>((false, "Seller cannot place bids on their own product", null));
            }

            int currentThreshold = (product.CurrentBid.HasValue && product.CurrentBid.Value > 0) ? product.CurrentBid.Value : product.StartingPrice;
            if (amount <= currentThreshold)
            {
                return Task.FromResult<(bool, string?, BidDto?)>((false, $"Bid must be greater than current bid ({currentThreshold:C})", null));
            }

            DateTime now = DateTime.UtcNow;
            string bidId = $"b-{Guid.NewGuid():N}";

            if (_winningBidByProduct.TryGetValue(productId, out string? previousWinningId) && !string.IsNullOrEmpty(previousWinningId))
            {
                if (_bids.TryGetValue(previousWinningId, out BidDto? previousBid) && previousBid is not null && previousBid.IsWinningBid)
                {
                    _bids[previousWinningId] = previousBid with { IsWinningBid = false };
                }
            }

            BidDto newBid = new(bidId, productId, bidderId, bidderUsername, amount, now, true);
            _bids[bidId] = newBid;
            _winningBidByProduct[productId] = bidId;

            product.CurrentBid = (int)amount;
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
                .Where(p =>
                    p.Bidders.Any(b => b.BidderId == bidderId) &&
                    p.Status != ProductStatus.Rejected &&
                    p.Feedback is null &&
                    (p.Status != ProductStatus.Expired || p.HighestBidderId == bidderId));

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

        public Task<(bool Success, string? Error, ProductDto? Product)> AddFeedbackAsync(string productId, FeedbackDto feedback)
        {
            ProductDto? product = Products.FirstOrDefault(p => p.Id == productId);
            if (product is null)
            {
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Product not found", null));
            }

            if (product.Status != ProductStatus.Sold)
            {
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Feedback can only be added to sold products", null));
            }

            if (!feedback.Rating.HasValue || feedback.Rating < 1 || feedback.Rating > 5)
            {
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Rating must be between 1 and 5", null));
            }

            product.Feedback = feedback;
            product.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<(bool, string?, ProductDto?)>((true, null, product));
        }

        public Task<IEnumerable<FeedbackItemDto>> GetFeedbackReceivedByUserAsync(string userId)
        {
            IEnumerable<FeedbackItemDto> feedbackItems = Products
                .Where(p =>
                    p.SellerId == userId &&
                    (p.Status == ProductStatus.Sold || p.Status == ProductStatus.Rejected) &&
                    p.Feedback is not null)
                .Select(p => new FeedbackItemDto(
                    $"feedback-{p.Id}",
                    p.Id ?? string.Empty,
                    p.Name ?? string.Empty,
                    p.Feedback!.Rating ?? 0,
                    p.Feedback.Comment,
                    p.UpdatedAt ?? p.CreatedAt,
                    p.HighestBidderUsername ?? string.Empty))
                .AsEnumerable();

            return Task.FromResult(feedbackItems);
        }

        public Task<(bool Success, string? Error, ProductDto? Product)> MarkAsRejectedAsync(string id, string? reason)
        {
            ProductDto? product = Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
            {
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Product not found", null));
            }

            product.Status = ProductStatus.Rejected;
            product.IsCompleted = false;
            product.TransactionStatus = null;
            product.Feedback = new FeedbackDto(
                null,
                reason);
            product.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<(bool, string?, ProductDto?)>((true, null, product));
        }

        public Task<(bool Success, string? Error, ProductDto? Product)> MarkAsAcceptedAsync(string id)
        {
            ProductDto? product = Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
            {
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Product not found", null));
            }

            product.Status = ProductStatus.Active;
            product.CurrentBid = product.StartingPrice;
            product.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<(bool, string?, ProductDto?)>((true, null, product));
        }
    }
}