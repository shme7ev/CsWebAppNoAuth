using Npgsql;
using WebAppNoAuth.Models;

namespace WebAppNoAuth.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<List<Product>> GetProductsByCategoryAsync(string category);
}

public class ProductService : IProductService
{
    private readonly string _connectionString;

    public ProductService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = new List<Product>();
        
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new NpgsqlCommand(
            "SELECT id, name, description, price, category, created_at FROM products ORDER BY name", 
            connection);
        
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Price = reader.GetDecimal(3),
                Category = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                CreatedAt = reader.GetDateTime(5)
            });
        }
        
        return products;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new NpgsqlCommand(
            "SELECT id, name, description, price, category, created_at FROM products WHERE id = @id", 
            connection);
        
        command.Parameters.AddWithValue("@id", id);
        
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Price = reader.GetDecimal(3),
                Category = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                CreatedAt = reader.GetDateTime(5)
            };
        }
        
        return null;
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        var products = new List<Product>();
        
        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new NpgsqlCommand(
            "SELECT id, name, description, price, category, created_at FROM products WHERE category = @category ORDER BY name", 
            connection);
        
        command.Parameters.AddWithValue("@category", category);
        
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Price = reader.GetDecimal(3),
                Category = reader.GetString(4),
                CreatedAt = reader.GetDateTime(5)
            });
        }
        
        return products;
    }
}
