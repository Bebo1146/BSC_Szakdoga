namespace ServicesHoster.Services
{
    public class ProductDto
    {
        public ProductDto() { }

        public string? Id { get; set; }
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
        public List<ProductBidderDto> Bidders { get; set; } = [];

        public string? SellerId { get; set; }
        public string? SellerUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsCompleted { get; set; }
        public TransactionStatus? TransactionStatus { get; set; }
        public FeedbackDto? Feedback { get; set; }

        public bool IsActive =>
            Status != ProductStatus.Sold &&
            Status != ProductStatus.Expired &&
            Status != ProductStatus.Cancelled &&
            Status != ProductStatus.Rejected &&
            DateTime.UtcNow >= AuctionStartTime &&
            DateTime.UtcNow < AuctionEndTime;

        public bool HasEnded =>
            DateTime.UtcNow >= AuctionEndTime ||
            Status == ProductStatus.Sold ||
            Status == ProductStatus.Expired ||
            Status == ProductStatus.Rejected;

        public TimeSpan? TimeRemaining =>
            HasEnded ? null : AuctionEndTime - DateTime.UtcNow;
    }

    public enum ProductStatus
    {
        Draft,
        Active,
        Sold,
        Expired,
        Cancelled,
        Rejected
    }

    public enum TransactionStatus
    {
        Pending,
        PaymentReceived,
        Shipped,
        Delivered,
        Completed,
        Cancelled
    }

    public record FeedbackDto(
        int? Rating,
        string? Comment
    );

    public record ProductBidderDto(
        string BidderId,
        string BidderUsername
    );

    public record RejectProductRequest(
        string Id,
        string? Reason
    );
}