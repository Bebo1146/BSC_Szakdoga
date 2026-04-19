namespace ServicesHoster.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(string id);
        Task AddRangeAsync(IEnumerable<ProductDto> products, string userName, string userPreferedName);
        Task<IEnumerable<ProductDto>> GetByUserAsync(string userId);
        Task<(bool Success, string? Error, BidDto? Bid)> PlaceBidAsync(string productId, int amount, string bidderId, string bidderUsername);
        Task<IEnumerable<BidDto>> GetBidsAsync(string productId);
        Task<IEnumerable<ProductDto>> GetProductsByBidderAsync(string bidderId);
        Task<(bool Success, string? Error, ProductDto? Product)> MarkAsSoldAsync(string id);
        Task<(bool Success, string? Error, ProductDto? Product)> MarkAsRejectedAsync(string id, string? reason);
        Task<(bool Success, string? Error, ProductDto? Product)> MarkAsAcceptedAsync(string id);
        Task<(bool Success, string? Error, ProductDto? Product)> AddFeedbackAsync(string productId, FeedbackDto feedback);
        Task<IEnumerable<FeedbackItemDto>> GetFeedbackReceivedByUserAsync(string userId);
        Task<IEnumerable<ProductDto>> GetActiveProductsAsync();
        Task ExpireEndedAuctionsAsync();
    }
}