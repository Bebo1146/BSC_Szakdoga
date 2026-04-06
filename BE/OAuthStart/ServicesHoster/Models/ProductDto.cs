namespace ServicesHoster.Services
{
    /// <summary>
    /// Represents a product listed for auction
    /// </summary>
    public class ProductDto
    {
        // Parameterless constructor for JSON deserialization
        public ProductDto() { }

        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public ProductStatus Status { get; set; }

        // Auction-specific fields
        public int StartingPrice { get; set; }
        public int? CurrentBid { get; set; }
        public DateTime AuctionStartTime { get; set; }
        public DateTime AuctionEndTime { get; set; }

        // Bidding information
        public int TotalBids { get; set; }
        public string? HighestBidderId { get; set; }
        public string? HighestBidderUsername { get; set; }
        public List<ProductBidderDto> Bidders { get; set; } = [];

        // Owner/Seller information
        public string? SellerId { get; set; }
        public string? SellerUsername { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Transaction & Feedback
        public bool IsCompleted { get; set; }
        public TransactionStatus? TransactionStatus { get; set; }
        public FeedbackDto? Feedback { get; set; }

        /// <summary>
        /// Checks if the auction is currently active
        /// </summary>
        public bool IsActive =>
            Status != ProductStatus.Sold &&
            Status != ProductStatus.Expired &&
            Status != ProductStatus.Cancelled &&
            Status != ProductStatus.Rejected &&
            DateTime.UtcNow >= AuctionStartTime &&
            DateTime.UtcNow < AuctionEndTime;

        /// <summary>
        /// Checks if the auction has ended
        /// </summary>
        public bool HasEnded =>
            DateTime.UtcNow >= AuctionEndTime ||
            Status == ProductStatus.Sold ||
            Status == ProductStatus.Expired ||
            Status == ProductStatus.Rejected;

        /// <summary>
        /// Time remaining until auction ends (null if ended)
        /// </summary>
        public TimeSpan? TimeRemaining =>
            HasEnded ? null : AuctionEndTime - DateTime.UtcNow;
    }

    /// <summary>
    /// Product/Auction status
    /// </summary>
    public enum ProductStatus
    {
        Draft,      // Not yet published
        Active,     // Currently listed and accepting bids
        Sold,       // Auction ended with successful sale
        Expired,    // Auction ended without meeting reserve price
        Cancelled,  // Cancelled by seller or admin
        Rejected    // Rejected by admin
    }

    /// <summary>
    /// Transaction status after auction completion
    /// </summary>
    public enum TransactionStatus
    {
        Pending,        // Auction won, awaiting payment
        PaymentReceived,// Payment confirmed
        Shipped,        // Item shipped to buyer
        Delivered,      // Item delivered
        Completed,      // Transaction fully completed
        Cancelled       // Transaction cancelled
    }

    /// <summary>
    /// Feedback from winner after transaction or admin rejection note
    /// </summary>
    public record FeedbackDto(
        int? Rating,
        string? Comment
    );

    /// <summary>
    /// Represents a bidder on the product auction
    /// </summary>
    public record ProductBidderDto(
        string BidderId,
        string BidderUsername
    );

    /// <summary>
    /// Request to reject a product with an optional reason
    /// </summary>
    public record RejectProductRequest(
        string Id,
        string? Reason
    );
}