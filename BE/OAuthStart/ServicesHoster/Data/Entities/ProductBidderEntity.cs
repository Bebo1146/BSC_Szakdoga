namespace ServicesHoster.Data.Entities
{
    public class ProductBidderEntity
    {
        public int Id { get; set; } // Auto-increment PK
        public string ProductId { get; set; } = string.Empty;
        public string BidderId { get; set; } = string.Empty;
        public string BidderUsername { get; set; } = string.Empty;

        // Navigation
        public ProductEntity Product { get; set; } = null!;
    }
}