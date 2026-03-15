namespace ServicesHoster.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(string id);
        Task AddRangeAsync(IEnumerable<ProductDto> products, string userName, string userPreferedName);
        Task<IEnumerable<ProductDto>> GetByUserAsync(string userId);

        // Place a bid on a product. Returns (success, error, created bid)
        Task<(bool Success, string? Error, BidDto? Bid)> PlaceBidAsync(string productId, decimal amount, string bidderId, string bidderUsername);

        // Get bid history for a product
        Task<IEnumerable<BidDto>> GetBidsAsync(string productId);

        // Get products that a given bidder has placed bids on
        Task<IEnumerable<ProductDto>> GetProductsByBidderAsync(string bidderId);
    }
}