using OAuthCodeFlowService.Configuration;
using OAuthCodeFlowService.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    // Example: builder.Services.AddSingleton<ISessionRepository>(_ => new RedisSessionRepository(redisConn));
    // (Add StackExchange.Redis and implement RedisSessionRepository as shown earlier if you want production scale)
    builder.Services.AddSingleton<ISessionRepository, InMemorySessionRepository>(); // fallback until Redis implemented
}
else
{
    builder.Services.AddSingleton<ISessionRepository, InMemorySessionRepository>();
}

builder.Services.AddHttpClient<ITokenService, TokenService>();
builder.Services.AddHttpClient(); // for proxying downstream APIs

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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// (Optional) Add session middleware here if you implement it
// app.UseSessionAuthentication();

app.UseAuthorization();
app.MapControllers();

app.Run();
