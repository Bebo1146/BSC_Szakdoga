using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicesHoster.Services
{
    public class InMemoryProductService : IProductService
    {
        private static readonly ConcurrentBag<ProductDto> Products = new(new[]
        {
            new ProductDto
            {
                Id = "p-1000",
                Name = "Classic Film Camera",
                Description = "A well-preserved 1960s film camera, great for collectors and photography enthusiasts.",
                Category = "Electronics",
                Status = ProductStatus.Active,
                StartingPrice = 50,
                CurrentBid = 75,
                AuctionStartTime = DateTime.UtcNow,
                AuctionEndTime = DateTime.UtcNow.AddMinutes(1),
                TotalBids = 3,
                HighestBidderId = "Boti Boti",
                HighestBidderUsername = "Boti",
                SellerId = "Beni Beni",
                SellerUsername = "Beni",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-1),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("Boti Boti", "Boti"),
                    new ProductBidderDto("user-456", "collector_user-456"),
                    new ProductBidderDto("user-123", "collector_user-123")
                }
            },
            new ProductDto
            {
                Id = "p-1001",
                Name = "Vintage Rangefinder",
                Description = "Rare rangefinder camera from the 1960s in excellent cosmetic condition.",
                Category = "Electronics",
                Status = ProductStatus.Active,
                StartingPrice = 50,
                CurrentBid = 75,
                AuctionStartTime = DateTime.UtcNow,
                AuctionEndTime = DateTime.UtcNow.AddMinutes(1),
                TotalBids = 3,
                HighestBidderId = "Beni Beni",
                HighestBidderUsername = "Beni",
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("user-456", "collector_user-456"),
                    new ProductBidderDto("Beni Beni", "Beni")
                },
                SellerId = "Boti Boti",
                SellerUsername = "Boti",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-1),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null
            },
            new ProductDto
            {
                Id = "p-1002",
                Name = "Antique Pocket Watch",
                Description = "Swiss-made pocket watch with gold plating and intricate movement.",
                Category = "Jewelry",
                Status = ProductStatus.Expired,
                StartingPrice = 200,
                CurrentBid = 350,
                AuctionStartTime = DateTime.UtcNow.AddDays(-4),
                AuctionEndTime = DateTime.UtcNow.AddHours(-1),
                TotalBids = 7,
                HighestBidderId = "user-123",
                HighestBidderUsername = "collector_joe",
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("user-123", "collector_joe"),
                    new ProductBidderDto("bob", "Bob K.")
                },
                SellerId = "Beni Beni",
                SellerUsername = "Beni",
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
                Description = "Designer leather handbag, limited edition. Excellent craftsmanship.",
                Category = "Fashion",
                Status = ProductStatus.Active,
                StartingPrice = 100,
                CurrentBid = 150,
                AuctionStartTime = DateTime.UtcNow.AddHours(-12),
                AuctionEndTime = DateTime.UtcNow.AddDays(2),
                TotalBids = 5,
                HighestBidderId = "user-789",
                HighestBidderUsername = "fashionista",
                SellerId = "Boti Boti",
                SellerUsername = "Boti",
                CreatedAt = DateTime.UtcNow.AddHours(-12),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-15),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>()
            },
            new ProductDto
            {
                Id = "p-1004",
                Name = "Vintage Vinyl Collection",
                Description = "Curated set of 20 vinyl records from the 1970s in good condition.",
                Category = "Music",
                Status = ProductStatus.Active,
                StartingPrice = 30,
                CurrentBid = 120,
                AuctionStartTime = DateTime.UtcNow.AddDays(-1),
                AuctionEndTime = DateTime.UtcNow.AddDays(3),
                TotalBids = 12,
                HighestBidderId = "user-321",
                HighestBidderUsername = "music_lover",
                SellerId = "Barki",
                SellerUsername = "Barki",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-20),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>()
            },
            new ProductDto
            {
                Id = "p-1005",
                Name = "Gaming Console (Like New)",
                Description = "Latest generation gaming console, barely used, includes original box.",
                Category = "Electronics",
                Status = ProductStatus.Active,
                StartingPrice = 250,
                CurrentBid = 250,
                AuctionStartTime = DateTime.UtcNow.AddHours(-3),
                AuctionEndTime = DateTime.UtcNow.AddDays(2),
                TotalBids = 1,
                HighestBidderId = "user-555",
                HighestBidderUsername = "gamer_A",
                SellerId = "charlie",
                SellerUsername = "Charlie D.",
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                UpdatedAt = DateTime.UtcNow,
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("user-555", "gamer_A")
                }
            },
            new ProductDto
            {
                Id = "p-1006",
                Name = "Original Oil Painting",
                Description = "Original oil painting by a local artist — signed and framed.",
                Category = "Art",
                Status = ProductStatus.Active,
                StartingPrice = 500,
                CurrentBid = 450,
                AuctionStartTime = DateTime.UtcNow.AddHours(-7),
                AuctionEndTime = DateTime.UtcNow.AddDays(3),
                TotalBids = 4,
                HighestBidderId = "bob",
                HighestBidderUsername = "Bob K.",
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("bob", "Bob K."),
                    new ProductBidderDto("user-654", "art_fan")
                },
                SellerId = "Felhasznalo",
                SellerUsername = "Felhasznalo",
                CreatedAt = DateTime.UtcNow.AddHours(-7),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null
            },
            new ProductDto
            {
                Id = "p-1008",
                Name = "Wireless Noise-Cancelling Headphones",
                Description = "Comfortable, long battery life, and excellent noise cancellation.",
                Category = "Electronics",
                Status = ProductStatus.Active,
                StartingPrice = 180,
                CurrentBid = 180,
                AuctionStartTime = DateTime.UtcNow.AddHours(-1),
                AuctionEndTime = DateTime.UtcNow.AddDays(1),
                TotalBids = 0,
                HighestBidderId = null,
                HighestBidderUsername = null,
                SellerId = "dana",
                SellerUsername = "Dana S.",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddHours(-1),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>()
            },
            // ── New seeded products ──
            new ProductDto
            {
                Id = "p-1009",
                Name = "Handmade Ceramic Vase",
                Description = "Beautiful hand-thrown ceramic vase with a deep blue glaze, signed by the potter.",
                Category = "Art",
                Status = ProductStatus.Active,
                StartingPrice = 40,
                CurrentBid = 65,
                AuctionStartTime = DateTime.UtcNow.AddHours(-3),
                AuctionEndTime = DateTime.UtcNow.AddDays(1),
                TotalBids = 4,
                HighestBidderId = "user-654",
                HighestBidderUsername = "art_fan",
                SellerId = "Beni Beni",
                SellerUsername = "Beni",
                CreatedAt = DateTime.UtcNow.AddHours(-3),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-20),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("user-654", "art_fan"),
                    new ProductBidderDto("bob", "Bob K.")
                }
            },
            new ProductDto
            {
                Id = "p-1010",
                Name = "Espresso Machine — Professional Grade",
                Description = "Commercial-quality espresso machine with dual boilers and PID temperature control.",
                Category = "Home & Kitchen",
                Status = ProductStatus.Active,
                StartingPrice = 600,
                CurrentBid = 850,
                AuctionStartTime = DateTime.UtcNow.AddDays(-1),
                AuctionEndTime = DateTime.UtcNow.AddDays(3),
                TotalBids = 8,
                HighestBidderId = "Boti Boti",
                HighestBidderUsername = "Boti",
                SellerId = "Barki",
                SellerUsername = "Barki",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-5),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("Boti Boti", "Boti"),
                    new ProductBidderDto("user-789", "fashionista"),
                    new ProductBidderDto("user-888", "mountain_biker")
                }
            },
            new ProductDto
            {
                Id = "p-1011",
                Name = "First Edition Sci-Fi Novel",
                Description = "Rare 1965 first edition in near-mint condition with original dust jacket.",
                Category = "Books",
                Status = ProductStatus.Active,
                StartingPrice = 120,
                CurrentBid = 210,
                AuctionStartTime = DateTime.UtcNow.AddHours(-8),
                AuctionEndTime = DateTime.UtcNow.AddDays(2),
                TotalBids = 5,
                HighestBidderId = "user-123",
                HighestBidderUsername = "collector_joe",
                SellerId = "charlie",
                SellerUsername = "Charlie D.",
                CreatedAt = DateTime.UtcNow.AddHours(-8),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("user-123", "collector_joe"),
                    new ProductBidderDto("Beni Beni", "Beni")
                }
            },
            new ProductDto
            {
                Id = "p-1012",
                Name = "Drone with 4K Camera",
                Description = "Foldable quadcopter drone with stabilized 4K camera and 30-minute flight time.",
                Category = "Electronics",
                Status = ProductStatus.Draft,
                StartingPrice = 350,
                CurrentBid = 350,
                AuctionStartTime = DateTime.UtcNow.AddHours(2),
                AuctionEndTime = DateTime.UtcNow.AddDays(5),
                TotalBids = 0,
                HighestBidderId = null,
                HighestBidderUsername = null,
                SellerId = "Boti Boti",
                SellerUsername = "Boti",
                CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>()
            },
            new ProductDto
            {
                Id = "p-1013",
                Name = "Signed Basketball Jersey",
                Description = "Authentic game-worn jersey signed by a Hall of Fame player, with certificate of authenticity.",
                Category = "Sports",
                Status = ProductStatus.Sold,
                StartingPrice = 400,
                CurrentBid = 1250,
                AuctionStartTime = DateTime.UtcNow.AddDays(-14),
                AuctionEndTime = DateTime.UtcNow.AddDays(-7),
                TotalBids = 18,
                HighestBidderId = "user-888",
                HighestBidderUsername = "mountain_biker",
                SellerId = "Felhasznalo",
                SellerUsername = "Felhasznalo",
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                UpdatedAt = DateTime.UtcNow.AddDays(-7),
                IsCompleted = true,
                TransactionStatus = TransactionStatus.Completed,
                Feedback = new FeedbackDto(4, "Authentic item, well packaged. Shipping took a bit longer than expected."),
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("user-888", "mountain_biker"),
                    new ProductBidderDto("user-555", "gamer_A"),
                    new ProductBidderDto("bob", "Bob K.")
                }
            },
            new ProductDto
            {
                Id = "p-1014",
                Name = "Vintage Mechanical Keyboard",
                Description = "Cherry MX Blue switches, full metal chassis, original keycaps from the early 1990s.",
                Category = "Electronics",
                Status = ProductStatus.Active,
                StartingPrice = 80,
                CurrentBid = 145,
                AuctionStartTime = DateTime.UtcNow.AddHours(-2),
                AuctionEndTime = DateTime.UtcNow.AddDays(1).AddHours(6),
                TotalBids = 7,
                HighestBidderId = "user-555",
                HighestBidderUsername = "gamer_A",
                SellerId = "dana",
                SellerUsername = "Dana S.",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-3),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("user-555", "gamer_A"),
                    new ProductBidderDto("Boti Boti", "Boti"),
                    new ProductBidderDto("user-321", "music_lover")
                }
            },
            new ProductDto
            {
                Id = "p-1015",
                Name = "Handcrafted Leather Wallet",
                Description = "Full-grain Italian leather bifold wallet, hand-stitched with waxed thread.",
                Category = "Fashion",
                Status = ProductStatus.Expired,
                StartingPrice = 25,
                CurrentBid = 55,
                AuctionStartTime = DateTime.UtcNow.AddDays(-5),
                AuctionEndTime = DateTime.UtcNow.AddHours(-6),
                TotalBids = 6,
                HighestBidderId = "Beni Beni",
                HighestBidderUsername = "Beni",
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("Beni Beni", "Beni"),
                    new ProductBidderDto("user-654", "art_fan")
                },
                SellerId = "Barki",
                SellerUsername = "Barki",
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddHours(-6),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null
            },
            new ProductDto
            {
                Id = "p-1016",
                Name = "Acoustic Guitar — Solid Spruce Top",
                Description = "Solid spruce top, rosewood back and sides. Rich tone, perfect for fingerpicking.",
                Category = "Music",
                Status = ProductStatus.Active,
                StartingPrice = 200,
                CurrentBid = 310,
                AuctionStartTime = DateTime.UtcNow.AddHours(-10),
                AuctionEndTime = DateTime.UtcNow.AddDays(3),
                TotalBids = 4,
                HighestBidderId = "user-321",
                HighestBidderUsername = "music_lover",
                SellerId = "Beni Beni",
                SellerUsername = "Beni",
                CreatedAt = DateTime.UtcNow.AddHours(-10),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-15),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("user-321", "music_lover"),
                    new ProductBidderDto("user-123", "collector_joe")
                }
            },
            new ProductDto
            {
                Id = "p-1017",
                Name = "Cast Iron Skillet Set",
                Description = "Pre-seasoned 3-piece cast iron skillet set (8\", 10\", 12\"). Oven safe to 500°F.",
                Category = "Home & Kitchen",
                Status = ProductStatus.Sold,
                StartingPrice = 45,
                CurrentBid = 90,
                AuctionStartTime = DateTime.UtcNow.AddDays(-6),
                AuctionEndTime = DateTime.UtcNow.AddDays(-2),
                TotalBids = 9,
                HighestBidderId = "user-789",
                HighestBidderUsername = "fashionista",
                SellerId = "Boti Boti",
                SellerUsername = "Boti",
                CreatedAt = DateTime.UtcNow.AddDays(-6),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                IsCompleted = true,
                TransactionStatus = TransactionStatus.Completed,
                Feedback = null,
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("user-789", "fashionista"),
                    new ProductBidderDto("Beni Beni", "Beni")
                }
            },
            new ProductDto
            {
                Id = "p-1018",
                Name = "Telescope — 8\" Dobsonian",
                Description = "Excellent deep-sky telescope for beginners and intermediates. Includes eyepiece kit.",
                Category = "Science",
                Status = ProductStatus.Draft,
                StartingPrice = 280,
                CurrentBid = 280,
                AuctionStartTime = DateTime.UtcNow.AddHours(5),
                AuctionEndTime = DateTime.UtcNow.AddDays(7),
                TotalBids = 0,
                HighestBidderId = null,
                HighestBidderUsername = null,
                SellerId = "Felhasznalo",
                SellerUsername = "Felhasznalo",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>()
            },
            new ProductDto
            {
                Id = "p-1019",
                Name = "Retro Polaroid Camera",
                Description = "Iconic instant camera from the 1980s, fully functional with original leather case.",
                Category = "Electronics",
                Status = ProductStatus.Active,
                StartingPrice = 60,
                CurrentBid = 95,
                AuctionStartTime = DateTime.UtcNow.AddHours(-2),
                AuctionEndTime = DateTime.UtcNow.AddMinutes(1),
                TotalBids = 4,
                HighestBidderId = "Boti Boti",
                HighestBidderUsername = "Boti",
                SellerId = "Beni Beni",
                SellerUsername = "Beni",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-1),
                IsCompleted = false,
                TransactionStatus = null,
                Feedback = null,
                Bidders = new List<ProductBidderDto>
                {
                    new ProductBidderDto("Boti Boti", "Boti"),
                    new ProductBidderDto("user-123", "collector_joe")
                }
            },
        });

        private static readonly ConcurrentDictionary<string, BidDto> _bids = new();
        private static readonly ConcurrentDictionary<string, string> _winningBidByProduct = new();

        public Task ExpireEndedAuctionsAsync()
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

            return Task.CompletedTask;
        }

        public Task<IEnumerable<ProductDto>> GetAllAsync() =>
            Task.FromResult(Products.AsEnumerable());

        public Task<IEnumerable<ProductDto>> GetActiveProductsAsync() =>
            Task.FromResult(Products.Where(p => p.Status == ProductStatus.Active).AsEnumerable());

        public Task<ProductDto?> GetByIdAsync(string id) =>
            Task.FromResult(Products.FirstOrDefault(p => p.Id == id));

        public Task AddRangeAsync(IEnumerable<ProductDto> products, string userName, string userPreferedName)
        {
            DateTime now = DateTime.UtcNow;

            foreach (ProductDto product in products)
            {
                ProductDto productWithUser = new()
                {
                    Id = $"p-{Guid.NewGuid():N}",
                    Name = product.Name,
                    Description = product.Description,
                    Category = product.Category,
                    Status = product.Status,
                    StartingPrice = product.StartingPrice,
                    CurrentBid = product.StartingPrice,
                    AuctionStartTime = product.AuctionStartTime,
                    AuctionEndTime = product.AuctionEndTime,
                    TotalBids = 0,
                    HighestBidderId = null,
                    HighestBidderUsername = null,
                    Bidders = new List<ProductBidderDto>(),
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

        public Task<IEnumerable<ProductDto>> GetByUserAsync(string userId) =>
            Task.FromResult(Products
                .Where(p => p.SellerId == userId && p.Status != ProductStatus.Rejected)
                .AsEnumerable());

        public Task<(bool Success, string? Error, BidDto? Bid)> PlaceBidAsync(string productId, int amount, string bidderId, string bidderUsername)
        {
            ProductDto? product = Products.FirstOrDefault(p => p.Id == productId);
            if (product is null)
                return Task.FromResult<(bool, string?, BidDto?)>((false, "Product not found", null));

            if (product.SellerId == bidderId)
                return Task.FromResult<(bool, string?, BidDto?)>((false, "Seller cannot place bids on their own product", null));

            int currentThreshold = (product.CurrentBid.HasValue && product.CurrentBid.Value > 0) ? product.CurrentBid.Value : product.StartingPrice;
            if (amount <= currentThreshold)
                return Task.FromResult<(bool, string?, BidDto?)>((false, $"Bid must be greater than current bid ({currentThreshold:C})", null));

            DateTime now = DateTime.UtcNow;
            string bidId = $"b-{Guid.NewGuid():N}";

            if (_winningBidByProduct.TryGetValue(productId, out string? previousWinningId) && !string.IsNullOrEmpty(previousWinningId))
            {
                if (_bids.TryGetValue(previousWinningId, out BidDto? previousBid) && previousBid is not null && previousBid.IsWinningBid)
                    _bids[previousWinningId] = previousBid with { IsWinningBid = false };
            }

            BidDto newBid = new(bidId, productId, bidderId, bidderUsername, amount, now, true);
            _bids[bidId] = newBid;
            _winningBidByProduct[productId] = bidId;

            product.CurrentBid = amount;
            product.TotalBids += 1;
            product.HighestBidderId = bidderId;
            product.HighestBidderUsername = bidderUsername;
            product.UpdatedAt = now;

            if (!product.Bidders.Any(b => b.BidderId == bidderId))
                product.Bidders.Add(new ProductBidderDto(bidderId, bidderUsername));

            return Task.FromResult<(bool, string?, BidDto?)>((true, null, newBid));
        }

        public Task<IEnumerable<BidDto>> GetBidsAsync(string productId) =>
            Task.FromResult(_bids.Values
                .Where(b => b.ProductId == productId)
                .OrderBy(b => b.BidTime)
                .AsEnumerable());

        public Task<IEnumerable<ProductDto>> GetProductsByBidderAsync(string bidderId) =>
            Task.FromResult(Products
                .Where(p =>
                    p.Bidders.Any(b => b.BidderId == bidderId) &&
                    p.Status != ProductStatus.Rejected &&
                    p.Feedback is null &&
                    (p.Status != ProductStatus.Expired || p.HighestBidderId == bidderId))
                .AsEnumerable());

        public Task<(bool Success, string? Error, ProductDto? Product)> MarkAsSoldAsync(string id)
        {
            ProductDto? product = Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Product not found", null));

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
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Product not found", null));

            if (product.Status != ProductStatus.Sold)
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Feedback can only be added to sold products", null));

            if (!feedback.Rating.HasValue || feedback.Rating < 1 || feedback.Rating > 5)
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Rating must be between 1 and 5", null));

            product.Feedback = feedback;
            product.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<(bool, string?, ProductDto?)>((true, null, product));
        }

        public Task<IEnumerable<FeedbackItemDto>> GetFeedbackReceivedByUserAsync(string userId) =>
            Task.FromResult(Products
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
                .AsEnumerable());

        public Task<(bool Success, string? Error, ProductDto? Product)> MarkAsRejectedAsync(string id, string? reason)
        {
            ProductDto? product = Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Product not found", null));

            product.Status = ProductStatus.Rejected;
            product.IsCompleted = false;
            product.TransactionStatus = null;
            product.Feedback = new FeedbackDto(null, reason);
            product.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<(bool, string?, ProductDto?)>((true, null, product));
        }

        public Task<(bool Success, string? Error, ProductDto? Product)> MarkAsAcceptedAsync(string id)
        {
            ProductDto? product = Products.FirstOrDefault(p => p.Id == id);
            if (product is null)
                return Task.FromResult<(bool, string?, ProductDto?)>((false, "Product not found", null));

            product.Status = ProductStatus.Active;
            product.CurrentBid = product.StartingPrice;
            product.UpdatedAt = DateTime.UtcNow;

            return Task.FromResult<(bool, string?, ProductDto?)>((true, null, product));
        }
    }
}