using Microsoft.EntityFrameworkCore;
using WebAppNoAuth.Data;
using WebAppNoAuth.Models;

namespace WebAppNoAuth.Services;

public interface IProductServiceEF
{
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<List<Product>> GetProductsByCategoryAsync(string category);
    Task<int> GetTotalProductsCountAsync();
}

public class ProductServiceEF(ApplicationDbContext context) : IProductServiceEF
{
    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await context.Products
            .OrderBy(p => p.Name)
            /* for custom query, you can use
             // .FromSql($@"SELECT * FROM products ORDER BY name")
             */
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await context.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(string category)
    {
        return await context.Products
            .Where(p => p.Category == category)
            .OrderBy(p => p.Name)
            /* for custom query, you can use
            // .FromSql($@"SELECT * FROM products where products.category = {category} ORDER BY name")
            */
            .ToListAsync();
    }

    public async Task<int> GetTotalProductsCountAsync()
    {
        return await context.Products.CountAsync();
    }
}
