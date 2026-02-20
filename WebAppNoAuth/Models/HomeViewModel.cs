using WebAppNoAuth.Models;

namespace WebAppNoAuth.Models;

public class HomeViewModel
{
    public List<Product> RawSqlProducts { get; set; } = new();
    public List<Product> EntityFrameworkProducts { get; set; } = new();
    public int RawSqlCount { get; set; }
    public int EntityFrameworkCount { get; set; }
}