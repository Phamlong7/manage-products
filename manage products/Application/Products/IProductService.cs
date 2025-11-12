using ManageProducts.Application.Common;
using ManageProducts.Contracts.Products;

namespace ManageProducts.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductResponse>> GetProductsAsync(GetProductsQuery query, CancellationToken cancellationToken);

    Task<Result<ProductResponse>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<ProductResponse>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken);

    Task<Result> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteProductAsync(Guid id, CancellationToken cancellationToken);
}

