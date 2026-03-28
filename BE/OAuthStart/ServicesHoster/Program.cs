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

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "http://localhost:5215", "https://localhost:7037")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

// Register Product Service based on configuration
string storageType = builder.Configuration.GetValue<string>("Storage:Type") ?? "InMemory";

switch (storageType.ToLower())
{
    case "inmemory":
        builder.Services.AddSingleton<IProductService, InMemoryProductService>();
        break;
    default:
        builder.Services.AddSingleton<IProductService, InMemoryProductService>();
        break;
}

builder.Services.AddHostedService<AuctionTimerService>();

// Add token validation (JWT Bearer authentication + authorization)
builder.Services.AddTokenValidation(builder.Configuration);

WebApplication app = builder.Build();

app.UseHttpsRedirection();

// Enable CORS - MUST be before authentication/authorization
app.UseCors("AllowAll");

// Add authentication middleware (must be before authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<AuctionHub>("/hubs/auction");

app.Run();
