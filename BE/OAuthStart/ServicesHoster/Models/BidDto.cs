namespace ServicesHoster.Services
{
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