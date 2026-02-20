using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebAppNoAuth.Models;

namespace WebAppNoAuth.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            // Explicitly map to lowercase table name to match the existing database
            entity.ToTable("products");

            // Map properties to lowercase column names to match database schema
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");

            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .IsRequired()
                .HasColumnName("price");

            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasColumnName("category");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
        });

        // no need to seed data
        // SeedData(modelBuilder);
    }

    private static DataBuilder<Product> SeedData(ModelBuilder modelBuilder)
    {
        return modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Laptop Computer",
                Description = "High-performance laptop with 16GB RAM and 512GB SSD",
                Price = 1299.99m,
                Category = "Electronics",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse with USB receiver",
                Price = 29.99m,
                Category = "Electronics",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 3,
                Name = "Coffee Mug",
                Description = "Ceramic coffee mug with company logo",
                Price = 12.50m,
                Category = "Office Supplies",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 4,
                Name = "Desk Lamp",
                Description = "Adjustable LED desk lamp with touch controls",
                Price = 45.00m,
                Category = "Office Supplies",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 5,
                Name = "Notebook",
                Description = "Spiral-bound notebook with 100 pages",
                Price = 8.99m,
                Category = "Stationery",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 6,
                Name = "Bluetooth Headphones",
                Description = "Noise-cancelling wireless headphones",
                Price = 199.99m,
                Category = "Electronics",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 7,
                Name = "Water Bottle",
                Description = "Stainless steel water bottle 32oz",
                Price = 24.99m,
                Category = "Accessories",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id = 8,
                Name = "Backpack",
                Description = "Water-resistant backpack with laptop compartment",
                Price = 59.99m,
                Category = "Accessories",
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
