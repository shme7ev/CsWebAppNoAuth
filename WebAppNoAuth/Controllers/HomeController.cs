using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAppNoAuth.Models;
using WebAppNoAuth.Services;
using Microsoft.Extensions.Logging;

namespace WebAppNoAuth.Controllers;

public class HomeController(
    IProductService productService,
    IProductServiceEF productServiceEf,
    ILogger<HomeController> logger)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        logger.LogDebug("HomeController.Index() called");
        
        var viewModel = new HomeViewModel();
        
        // Get products using raw SQL
        logger.LogDebug("Fetching products using raw SQL");
        viewModel.RawSqlProducts = await productService.GetAllProductsAsync();
        viewModel.RawSqlCount = viewModel.RawSqlProducts.Count;
        logger.LogDebug($"Retrieved {viewModel.RawSqlCount} products using raw SQL");
        
        // Get products using Entity Framework
        logger.LogDebug("Fetching products using Entity Framework");
        viewModel.EntityFrameworkProducts = await productServiceEf.GetAllProductsAsync();
        viewModel.EntityFrameworkCount = viewModel.EntityFrameworkProducts.Count;
        logger.LogDebug($"Retrieved {viewModel.EntityFrameworkCount} products using Entity Framework");
        
        logger.LogDebug($"Home page loaded with {viewModel.RawSqlCount} (SQL) and {viewModel.EntityFrameworkCount} (EF) products");
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        logger.LogDebug("HomeController.Privacy() called");
        logger.LogDebug("Privacy page accessed");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        logger.LogDebug("HomeController.Error() called");
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        logger.LogError("Error occurred. Request ID: {RequestId}", requestId);
        return View(new ErrorViewModel { RequestId = requestId });
    }
}
