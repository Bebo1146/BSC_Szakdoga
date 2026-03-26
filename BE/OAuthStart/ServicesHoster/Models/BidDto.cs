namespace ServicesHoster.Services
{
    /// <summary>
    /// Represents a bid on an auction
    /// </summary>
    public record BidDto(
        string Id,
        string ProductId,
        string BidderId,
        string BidderUsername,
        int Amount,
        DateTime BidTime,
        bool IsWinningBid
    );
}