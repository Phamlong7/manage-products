using ManageProducts.Application.Common.Interfaces;
using ManageProducts.Domain.Entities;
using ManageProducts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ManageProducts.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(string? search, decimal? minPrice, decimal? maxPrice, string? sortOrder, CancellationToken cancellationToken)
    {
        IQueryable<Product> query = _context.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(product => EF.Functions.ILike(product.Name, term));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(product => product.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(product => product.Price <= maxPrice.Value);
        }

        query = sortOrder switch
        {
            "price_asc" => query.OrderBy(product => product.Price),
            "price_desc" => query.OrderByDescending(product => product.Price),
            "name_asc" => query.OrderBy(product => product.Name),
            "name_desc" => query.OrderByDescending(product => product.Name),
            _ => query.OrderBy(product => product.Name)
        };

        return await query.ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Products.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken) =>
        await _context.Products.AddAsync(product, cancellationToken);

    public Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product, CancellationToken cancellationToken)
    {
        _context.Products.Remove(product);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken)
    {
        var query = _context.Products.AsNoTracking()
            .Where(product => EF.Functions.ILike(product.Name, name.Trim()));

        if (excludeId.HasValue)
        {
            query = query.Where(product => product.Id != excludeId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}

