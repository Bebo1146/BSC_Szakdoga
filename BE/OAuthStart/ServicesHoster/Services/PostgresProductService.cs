using Microsoft.EntityFrameworkCore;
using ServicesHoster.Data;
using ServicesHoster.Data.Entities;

namespace ServicesHoster.Services
{
    public class PostgresProductService : IProductService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public PostgresProductService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private AuctionDbContext CreateContext()
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            await using AuctionDbContext db = CreateContext();
            List<ProductEntity> entities = await db.Products
                .Include(p => p.Bidders)
                .AsNoTracking()
                .ToListAsync();
            return entities.Select(MapToDto);
        }

        public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
        {
            await using AuctionDbContext db = CreateContext();
            List<ProductEntity> entities = await db.Products
                .Where(p => p.Status == ProductStatus.Active)
                .AsNoTracking()
                .ToListAsync();
            return entities.Select(MapToDto);
        }

        public async Task<ProductDto?> GetByIdAsync(string id)
        {
            await using AuctionDbContext db = CreateContext();
            ProductEntity? entity = await db.Products
                .Include(p => p.Bidders)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task AddRangeAsync(IEnumerable<ProductDto> products, string userName, string userPreferedName)
        {
            await using AuctionDbContext db = CreateContext();
            DateTime now = DateTime.UtcNow;

            foreach (ProductDto product in products)
            {
                ProductEntity entity = new()
                {
                    Id = $"p-{Guid.NewGuid():N}",
                    Name = product.Name,
                    Description = product.Description,
                    Category = product.Category,
                    Status = product.Status,
                    ImageUrl = product.ImageUrl,
                    StartingPrice = product.StartingPrice,
                    CurrentBid = product.StartingPrice,
                    AuctionStartTime = DateTime.SpecifyKind(product.AuctionStartTime, DateTimeKind.Utc),
                    AuctionEndTime = DateTime.SpecifyKind(product.AuctionEndTime, DateTimeKind.Utc),
                    TotalBids = 0,
                    SellerId = userName,
                    SellerUsername = userPreferedName,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsCompleted = false
                };
                db.Products.Add(entity);
            }

            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetByUserAsync(string userId)
        {
            await using AuctionDbContext db = CreateContext();
            List<ProductEntity> entities = await db.Products
                .Include(p => p.Bidders)
                .Where(p => p.SellerId == userId && p.Status != ProductStatus.Rejected)
                .AsNoTracking()
                .ToListAsync();
            return entities.Select(MapToDto);
        }

        public async Task<(bool Success, string? Error, BidDto? Bid)> PlaceBidAsync(string productId, int amount, string bidderId, string bidderUsername)
        {
            await using AuctionDbContext db = CreateContext();
            ProductEntity? product = await db.Products
                .Include(p => p.Bidders)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product is null)
                return (false, "Product not found", null);

            if (product.SellerId == bidderId)
                return (false, "Seller cannot place bids on their own product", null);

            int currentThreshold = (product.CurrentBid.HasValue && product.CurrentBid.Value > 0) ? product.CurrentBid.Value : product.StartingPrice;
            if (amount <= currentThreshold)
                return (false, $"Bid must be greater than current bid ({currentThreshold:C})", null);

            // Un-mark previous winning bid
            BidEntity? previousWinner = await db.Bids
                .Where(b => b.ProductId == productId && b.IsWinningBid)
                .FirstOrDefaultAsync();
            if (previousWinner is not null)
                previousWinner.IsWinningBid = false;

            DateTime now = DateTime.UtcNow;
            string bidId = $"b-{Guid.NewGuid():N}";

            BidEntity newBid = new()
            {
                Id = bidId,
                ProductId = productId,
                BidderId = bidderId,
                BidderUsername = bidderUsername,
                Amount = amount,
                BidTime = now,
                IsWinningBid = true
            };
            db.Bids.Add(newBid);

            product.CurrentBid = amount;
            product.TotalBids += 1;
            product.HighestBidderId = bidderId;
            product.HighestBidderUsername = bidderUsername;
            product.UpdatedAt = now;

            if (!product.Bidders.Any(b => b.BidderId == bidderId))
            {
                product.Bidders.Add(new ProductBidderEntity
                {
                    ProductId = productId,
                    BidderId = bidderId,
                    BidderUsername = bidderUsername
                });
            }

            await db.SaveChangesAsync();

            BidDto bidDto = new(bidId, productId, bidderId, bidderUsername, amount, now, true);
            return (true, null, bidDto);
        }

        public async Task<IEnumerable<BidDto>> GetBidsAsync(string productId)
        {
            await using AuctionDbContext db = CreateContext();
            return await db.Bids
                .Where(b => b.ProductId == productId)
                .OrderBy(b => b.BidTime)
                .Select(b => new BidDto(b.Id, b.ProductId, b.BidderId, b.BidderUsername, b.Amount, b.BidTime, b.IsWinningBid))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByBidderAsync(string bidderId)
        {
            await using AuctionDbContext db = CreateContext();
            List<ProductEntity> entities = await db.Products
                .Include(p => p.Bidders)
                .Where(p =>
                    p.Bidders.Any(b => b.BidderId == bidderId) &&
                    p.Status != ProductStatus.Rejected &&
                    p.FeedbackRating == null && p.FeedbackComment == null &&
                    (p.Status != ProductStatus.Expired || p.HighestBidderId == bidderId))
                .AsNoTracking()
                .ToListAsync();
            return entities.Select(MapToDto);
        }

        public async Task<(bool Success, string? Error, ProductDto? Product)> MarkAsSoldAsync(string id)
        {
            await using AuctionDbContext db = CreateContext();
            ProductEntity? product = await db.Products.Include(p => p.Bidders).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return (false, "Product not found", null);

            product.Status = ProductStatus.Sold;
            product.IsCompleted = true;
            product.TransactionStatus = TransactionStatus.Completed;
            product.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return (true, null, MapToDto(product));
        }

        public async Task<(bool Success, string? Error, ProductDto? Product)> MarkAsRejectedAsync(string id, string? reason)
        {
            await using AuctionDbContext db = CreateContext();
            ProductEntity? product = await db.Products.Include(p => p.Bidders).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return (false, "Product not found", null);

            product.Status = ProductStatus.Rejected;
            product.IsCompleted = false;
            product.TransactionStatus = null;
            product.FeedbackRating = null;
            product.FeedbackComment = reason;
            product.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return (true, null, MapToDto(product));
        }

        public async Task<(bool Success, string? Error, ProductDto? Product)> MarkAsAcceptedAsync(string id)
        {
            await using AuctionDbContext db = CreateContext();
            ProductEntity? product = await db.Products.Include(p => p.Bidders).FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) return (false, "Product not found", null);

            product.Status = ProductStatus.Active;
            product.CurrentBid = product.StartingPrice;
            product.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return (true, null, MapToDto(product));
        }

        public async Task<(bool Success, string? Error, ProductDto? Product)> AddFeedbackAsync(string productId, FeedbackDto feedback)
        {
            await using AuctionDbContext db = CreateContext();
            ProductEntity? product = await db.Products.Include(p => p.Bidders).FirstOrDefaultAsync(p => p.Id == productId);
            if (product is null) return (false, "Product not found", null);
            if (product.Status != ProductStatus.Sold) return (false, "Feedback can only be added to sold products", null);
            if (!feedback.Rating.HasValue || feedback.Rating < 1 || feedback.Rating > 5) return (false, "Rating must be between 1 and 5", null);

            product.FeedbackRating = feedback.Rating;
            product.FeedbackComment = feedback.Comment;
            product.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return (true, null, MapToDto(product));
        }

        public async Task<IEnumerable<FeedbackItemDto>> GetFeedbackReceivedByUserAsync(string userId)
        {
            await using AuctionDbContext db = CreateContext();
            return await db.Products
                .Where(p =>
                    p.SellerId == userId &&
                    (p.Status == ProductStatus.Sold || p.Status == ProductStatus.Rejected) &&
                    (p.FeedbackRating != null || p.FeedbackComment != null))
                .Select(p => new FeedbackItemDto(
                    $"feedback-{p.Id}",
                    p.Id,
                    p.Name ?? string.Empty,
                    p.FeedbackRating ?? 0,
                    p.FeedbackComment,
                    p.UpdatedAt ?? p.CreatedAt,
                    p.HighestBidderUsername ?? string.Empty))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task ExpireEndedAuctionsAsync()
        {
            await using AuctionDbContext db = CreateContext();
            DateTime now = DateTime.UtcNow;

            List<ProductEntity> expired = await db.Products
                .Where(p => p.Status == ProductStatus.Active && p.AuctionEndTime <= now)
                .ToListAsync();

            foreach (ProductEntity product in expired)
            {
                product.Status = ProductStatus.Expired;
                product.UpdatedAt = now;
            }

            await db.SaveChangesAsync();
        }

        // ── Mapping ──
        private static ProductDto MapToDto(ProductEntity e) => new()
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            Category = e.Category,
            Status = e.Status,
            ImageUrl = e.ImageUrl,
            StartingPrice = e.StartingPrice,
            CurrentBid = e.CurrentBid,
            AuctionStartTime = e.AuctionStartTime,
            AuctionEndTime = e.AuctionEndTime,
            TotalBids = e.TotalBids,
            HighestBidderId = e.HighestBidderId,
            HighestBidderUsername = e.HighestBidderUsername,
            Bidders = e.Bidders.Select(b => new ProductBidderDto(b.BidderId, b.BidderUsername)).ToList(),
            SellerId = e.SellerId,
            SellerUsername = e.SellerUsername,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            IsCompleted = e.IsCompleted,
            TransactionStatus = e.TransactionStatus,
            Feedback = (e.FeedbackRating is not null || e.FeedbackComment is not null)
                ? new FeedbackDto(e.FeedbackRating, e.FeedbackComment)
                : null
        };
    }
}