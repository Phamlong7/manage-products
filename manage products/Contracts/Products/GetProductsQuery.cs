namespace ManageProducts.Contracts.Products;

public sealed record GetProductsQuery(
    string? Search,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Sort);

