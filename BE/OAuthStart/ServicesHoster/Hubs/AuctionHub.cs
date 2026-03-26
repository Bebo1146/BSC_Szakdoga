using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ServicesHoster.Hubs
{
    [Authorize]
    public class AuctionHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }

    public record AuctionTimeUpdate(
        string ProductId,
        int TimeRemainingSeconds,
        bool IsActive,
        bool HasEnded,
        string Status
    );
}