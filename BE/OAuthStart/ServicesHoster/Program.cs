using Microsoft.EntityFrameworkCore;
using ServicesHoster.Data;
using ServicesHoster.Hubs;
using ServicesHoster.Services;
using TokenValidation.TokenValidation.ExtensionMethods;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddSignalR();

// Register Product Service based on configuration
string storageType = builder.Configuration.GetValue<string>("Storage:Type") ?? "InMemory";

switch (storageType.ToLowerInvariant())
{
    case "postgres":
        string connectionString = builder.Configuration.GetConnectionString("AuctionDb")
            ?? throw new InvalidOperationException("ConnectionStrings:AuctionDb is required when Storage:Type is Postgres");
        builder.Services.AddDbContext<AuctionDbContext>(options =>
            options.UseNpgsql(connectionString));
        builder.Services.AddScoped<IProductService, PostgresProductService>();
        break;
    default:
        builder.Services.AddSingleton<IProductService, InMemoryProductService>();
        break;
}

builder.Services.AddHostedService<AuctionTimerService>();

// Add token validation (JWT Bearer authentication + authorization)
builder.Services.AddTokenValidation(builder.Configuration);

WebApplication app = builder.Build();

// Auto-create database tables on startup (dev only)
if (storageType.Equals("postgres", StringComparison.OrdinalIgnoreCase))
{
    using IServiceScope scope = app.Services.CreateScope();
    AuctionDbContext db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Add authentication middleware (must be before authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<AuctionHub>("/hubs/auction");

app.Run();
