namespace MockApi.Contracts;

public record ProductDto(
    string Id,
    string Name,
    string Status,
    string Category,
    string InventoryText,
    string Vendor
);
