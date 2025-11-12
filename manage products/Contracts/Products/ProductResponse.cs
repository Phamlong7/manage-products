namespace ManageProducts.Contracts.Products;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    int StockQuantity,
    string? CategoryImageUrl,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

