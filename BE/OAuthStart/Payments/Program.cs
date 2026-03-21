using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
var app = builder.Build();

app.UseCors();

app.MapPost("/api/payments", async (HttpRequest request) =>
{
    var req = await request.ReadFromJsonAsync<PaymentRequest>() ?? new PaymentRequest();
    var id = "fake_pi_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    var res = new PaymentResponse
    {
        Id = id,
        Status = "requires_confirmation",
        ClientSecret = "fake_client_secret_" + id,
        PaymentUrl = $"http://localhost:4200/fake-checkout?paymentId={Uri.EscapeDataString(id)}"
    };
    return Results.Json(res);
});

app.MapPost("/api/payments/{id}/confirm", (string id) =>
{
    // simulate webhook / confirmation
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