using ManageProducts.Application.Common;
using ManageProducts.Application.Common.Interfaces;
using ManageProducts.Contracts.Products;
using ManageProducts.Domain.Entities;

namespace ManageProducts.Application.Products;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProductResponse>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(
            query.Search,
            query.MinPrice,
            query.MaxPrice,
            query.Sort,
            cancellationToken);

        return products.Select(MapToResponse).ToList();
    }

    public async Task<Result<ProductResponse>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        return product is null
            ? Result<ProductResponse>.Failure("Product not found.")
            : Result<ProductResponse>.Success(MapToResponse(product));
    }

    public async Task<Result<ProductResponse>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await ValidateAsync(request.Name, request.Price, request.StockQuantity, null, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result<ProductResponse>.Failure(validationResult.Error!);
        }

        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = decimal.Round(request.Price, 2, MidpointRounding.AwayFromZero),
            StockQuantity = request.StockQuantity,
            CategoryImageUrl = string.IsNullOrWhiteSpace(request.CategoryImageUrl)
                ? null
                : request.CategoryImageUrl.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        await _repository.AddAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<ProductResponse>.Success(MapToResponse(product));
    }

    public async Task<Result> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result.Failure("Product not found.");
        }

        var validationResult = await ValidateAsync(request.Name, request.Price, request.StockQuantity, id, cancellationToken);
        if (validationResult.IsFailure)
        {
            return validationResult;
        }

        product.Name = request.Name.Trim();
        product.Price = decimal.Round(request.Price, 2, MidpointRounding.AwayFromZero);
        product.StockQuantity = request.StockQuantity;
        product.CategoryImageUrl = string.IsNullOrWhiteSpace(request.CategoryImageUrl)
            ? null
            : request.CategoryImageUrl.Trim();
        product.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result.Failure("Product not found.");
        }

        await _repository.DeleteAsync(product, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result> ValidateAsync(string name, decimal price, int stockQuantity, Guid? excludeId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure("Name is required.");
        }

        if (price <= 0)
        {
            return Result.Failure("Price must be a positive number.");
        }

        if (stockQuantity < 0)
        {
            return Result.Failure("Stock quantity must be a non-negative integer.");
        }

        var nameExists = await _repository.ExistsByNameAsync(name.Trim(), excludeId, cancellationToken);
        if (nameExists)
        {
            return Result.Failure("A product with the same name already exists.");
        }

        return Result.Success();
    }

    private static ProductResponse MapToResponse(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Price,
            product.StockQuantity,
            product.CategoryImageUrl,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);
}

