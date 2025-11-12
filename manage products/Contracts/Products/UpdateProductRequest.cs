using System.ComponentModel.DataAnnotations;

namespace ManageProducts.Contracts.Products;

public sealed record UpdateProductRequest(
    [property: Required]
    string Name,

    [property: Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    decimal Price,

    [property: Range(0, int.MaxValue)]
    int StockQuantity,

    string? CategoryImageUrl);

