namespace ServicesHoster.Services
{
    public record FeedbackItemDto(
        string Id,
        string ProductId,
        string ProductName,
        int Rating,
        string? Comment,
        DateTime CreatedAt,
        string BuyerUsername
    );
}