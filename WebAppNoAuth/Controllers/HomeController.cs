using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAppNoAuth.Models;
using WebAppNoAuth.Services;

namespace WebAppNoAuth.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductServiceEF _productServiceEF;

    public HomeController(IProductService productService, IProductServiceEF productServiceEF)
    {
        _productService = productService;
        _productServiceEF = productServiceEF;
    }
    
    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeViewModel();
        
        // Get products using raw SQL
        viewModel.RawSqlProducts = await _productService.GetAllProductsAsync();
        viewModel.RawSqlCount = viewModel.RawSqlProducts.Count;
        
        // Get products using Entity Framework
        viewModel.EntityFrameworkProducts = await _productServiceEF.GetAllProductsAsync();
        viewModel.EntityFrameworkCount = viewModel.EntityFrameworkProducts.Count;
        
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
