using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAppNoAuth.Models;
using WebAppNoAuth.Services;
using Microsoft.Extensions.Logging;

namespace WebAppNoAuth.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductServiceEF _productServiceEF;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IProductService productService, IProductServiceEF productServiceEF, ILogger<HomeController> logger)
    {
        _productService = productService;
        _productServiceEF = productServiceEF;
        _logger = logger;
    }
    
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("HomeController.Index() called - Processing home page request");
        _logger.LogDebug("HomeController.Index() called");
        
        var viewModel = new HomeViewModel();
        
        // Get products using raw SQL
        _logger.LogDebug("Fetching products using raw SQL");
        viewModel.RawSqlProducts = await _productService.GetAllProductsAsync();
        viewModel.RawSqlCount = viewModel.RawSqlProducts.Count;
        _logger.LogDebug("Retrieved products using raw SQL {@ProductCount}", viewModel.RawSqlCount);
        
        // Get products using Entity Framework
        _logger.LogDebug("Fetching products using Entity Framework");
        viewModel.EntityFrameworkProducts = await _productServiceEF.GetAllProductsAsync();
        viewModel.EntityFrameworkCount = viewModel.EntityFrameworkProducts.Count;
        _logger.LogDebug($"Retrieved {viewModel.EntityFrameworkCount} products using Entity Framework");
        
        _logger.LogDebug($"Home page loaded with {viewModel.RawSqlCount} (SQL) and {viewModel.EntityFrameworkCount} (EF) products");
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        _logger.LogDebug("HomeController.Privacy() called");
        _logger.LogDebug("Privacy page accessed");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        _logger.LogDebug("HomeController.Error() called");
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        _logger.LogError("Error occurred in HomeController. Request ID: {RequestId}, Path: {RequestPath}", 
            requestId, HttpContext.Request.Path);
        return View(new ErrorViewModel { RequestId = requestId });
    }
}
