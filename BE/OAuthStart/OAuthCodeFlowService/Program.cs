using OAuthCodeFlowService.Configuration;
using OAuthCodeFlowService.Hubs;
using OAuthCodeFlowService.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure OAuth settings
builder.Services.Configure<OAuthSettings>(
    builder.Configuration.GetSection("OAuth"));

// Register services
builder.Services.AddSingleton<IPkceService, PkceService>();
builder.Services.AddSingleton<IAuthorizationStateStore, InMemoryAuthorizationStateStore>();

// Session repository: use Redis if configured, otherwise in-memory for dev
string? redisConn = builder.Configuration.GetValue<string>("Redis:ConnectionString");
if (!string.IsNullOrEmpty(redisConn))
{
    builder.Services.AddSingleton<ISessionRepository, InMemorySessionRepository>(); // fallback until Redis implemented
}
else
{
    builder.Services.AddSingleton<ISessionRepository, InMemorySessionRepository>();
}

builder.Services.AddHttpClient<ITokenService, TokenService>();
builder.Services.AddHttpClient(); // for proxying downstream APIs

builder.Services.AddSignalR();

// Add CORS for frontend applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4200" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

WebApplication app = builder.Build();

app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();
app.MapHub<AuctionHub>("/hubs/auction");

app.Run();
