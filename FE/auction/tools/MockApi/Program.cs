using MockApi.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Allow Angular dev server to call this API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p =>
        p.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod()
    );
});

var app = builder.Build();

app.UseCors();

// Hardcoded mock data (matches your FE exactly)
var products = new List<ProductDto>
{
    new("p-1001", "Yellow Sweater Set", "Active", "Set",      "15 in stock for 2 variants", "S.laz Store"),
    new("p-1002", "Merraid Top",        "Active", "Tops",     "8 in stock for 2 variants",  "S.laz Store"),
    new("p-1003", "Summer Bag",         "Active", "Handbag",  "25 in stock for 2 variants", "S.laz Store"),
    new("p-1004", "Lizzy Jacket",       "Active", "Jackets",  "5 in stock for 3 variants",  "S.laz Store"),
    new("p-1005", "Stripes Trousers",   "Active", "Trousers", "2 in stock for 2 variants",  "S.laz Store"),
    new("p-1006", "Sunny Sweeter",      "Draft",  "Top",      "5 in stock for 2 variants",  "S.laz Store"),
    new("p-1007", "Linen Shirt",        "Active", "Tops",     "10 in stock for 2 variants", "S.laz Store"),
};

// Endpoints
app.MapGet("/health", () => Results.Ok("OK"));

app.MapGet("/api/products", () =>
{
    return Results.Ok(products);
});

app.MapGet("/api/products/{id}", (string id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.Run();
