using Microsoft.AspNetCore.SignalR;
using ServicesHoster.Hubs;

namespace ServicesHoster.Services
{
    public class AuctionTimerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly ILogger<AuctionTimerService> _logger;

        public AuctionTimerService(
            IServiceScopeFactory scopeFactory,
            IHubContext<AuctionHub> hubContext,
            ILogger<AuctionTimerService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AuctionTimerService started.");

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using IServiceScope scope = _scopeFactory.CreateScope();
                    IProductService productService = scope.ServiceProvider.GetRequiredService<IProductService>();

                    await productService.ExpireEndedAuctionsAsync();

                    IEnumerable<ProductDto> activeProducts = await productService.GetAllAsync();

                    List<AuctionTimeUpdate> updates = [];
                    DateTime now = DateTime.UtcNow;

                    foreach (ProductDto product in activeProducts)
                    {
                        double remaining = (product.AuctionEndTime - now).TotalSeconds;
                        int secondsLeft = remaining > 0 ? (int)remaining : 0;

                        updates.Add(new AuctionTimeUpdate(
                            product.Id!,
                            secondsLeft,
                            product.IsActive,
                            product.HasEnded,
                            product.Status.ToString()
                        ));
                    }

                    if (updates.Count > 0)
                    {
                        await _hubContext.Clients.All.SendAsync("AuctionTimeUpdate", updates, stoppingToken);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Error in AuctionTimerService tick");
                }
            }
        }
    }
}