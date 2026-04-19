using ServicesHoster.Services;

namespace ServicesHoster.Data.Entities
{
    public class ProductEntity
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public ProductStatus Status { get; set; }
        public int StartingPrice { get; set; }
        public int? CurrentBid { get; set; }
        public DateTime AuctionStartTime { get; set; }
        public DateTime AuctionEndTime { get; set; }
        public int TotalBids { get; set; }
        public string? HighestBidderId { get; set; }
        public string? HighestBidderUsername { get; set; }
        public string? SellerId { get; set; }
        public string? SellerUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsCompleted { get; set; }
        public TransactionStatus? TransactionStatus { get; set; }
        public int? FeedbackRating { get; set; }
        public string? FeedbackComment { get; set; }
        public List<BidEntity> Bids { get; set; } = [];
        public List<ProductBidderEntity> Bidders { get; set; } = [];
    }
}