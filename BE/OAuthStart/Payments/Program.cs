using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TokenValidation.TokenValidation.ExtensionMethods;

var builder = WebApplication.CreateBuilder(args);

// JWT validation — same pattern as ServicesHoster
builder.Services.AddTokenValidation(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/payments", [Authorize] async (HttpRequest request) =>
{
    var req = await request.ReadFromJsonAsync<PaymentRequest>() ?? new PaymentRequest();
    var id = "fake_pi_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    var res = new PaymentResponse
    {
        Id = id,
        Status = "requires_confirmation",
        ClientSecret = "fake_client_secret_" + id,
        PaymentUrl = $"https://auction.local:9443/fake-checkout?paymentId={Uri.EscapeDataString(id)}"
    };
    return Results.Json(res);
});

app.MapPost("/api/payments/{id}/confirm", [Authorize] (string id) =>
{
    var res = new { id, status = "succeeded" };
    return Results.Json(res);
});

app.Run();

internal class PaymentRequest
{
    public string? BidId { get; set; }
    public decimal Amount { get; set; }
    public string? Method { get; set; }
}

internal class PaymentResponse
{
    public string? Id { get; set; }
    public string? Status { get; set; }
    public string? ClientSecret { get; set; }
    public string? PaymentUrl { get; set; }
}