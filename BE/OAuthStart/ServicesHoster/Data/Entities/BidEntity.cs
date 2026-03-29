namespace ServicesHoster.Data.Entities
{
    public class BidEntity
    {
        public string Id { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string BidderId { get; set; } = string.Empty;
        public string BidderUsername { get; set; } = string.Empty;
        public int Amount { get; set; }
        public DateTime BidTime { get; set; }
        public bool IsWinningBid { get; set; }

        // Navigation
        public ProductEntity Product { get; set; } = null!;
    }
}