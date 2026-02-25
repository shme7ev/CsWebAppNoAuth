namespace WebAppNoAuth.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Parameterless constructor
    public Product() { }

    // All-args constructor
    public Product(int id, string name, string description, decimal price, string category, DateTime createdAt)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        CreatedAt = createdAt;
    }
}