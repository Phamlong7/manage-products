using ManageProducts.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManageProducts.Infrastructure.Persistence;

public sealed class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(builder =>
        {
            builder.ToTable("products");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(p => p.Price)
                .HasColumnType("numeric(18,2)");
            builder.Property(p => p.StockQuantity)
                .IsRequired();
            builder.Property(p => p.CategoryImageUrl)
                .HasMaxLength(2048);
            builder.Property(p => p.CreatedAtUtc)
                .HasColumnName("created_at_utc")
                .IsRequired();
            builder.Property(p => p.UpdatedAtUtc)
                .HasColumnName("updated_at_utc")
                .IsRequired();
            builder.HasIndex(p => p.Name)
                .IsUnique();
        });
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<Product>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
    }
}

