using System.ComponentModel.DataAnnotations;

namespace ManageProducts.Contracts.Products;

public sealed record UpdateProductRequest(
    [Required]
    string Name,

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    decimal Price,

    [Range(0, int.MaxValue)]
    int StockQuantity,

    string? CategoryImageUrl);

