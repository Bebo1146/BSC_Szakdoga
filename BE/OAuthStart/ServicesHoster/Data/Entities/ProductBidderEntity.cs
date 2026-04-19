namespace ServicesHoster.Data.Entities
{
    public class ProductBidderEntity
    {
        public int Id { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string BidderId { get; set; } = string.Empty;
        public string BidderUsername { get; set; } = string.Empty;
        public ProductEntity Product { get; set; } = null!;
    }
}