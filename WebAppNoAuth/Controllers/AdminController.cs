using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppNoAuth.Models;
using WebAppNoAuth.Services;

namespace WebAppNoAuth.Controllers;

[Authorize] // This attribute requires JWT authentication for all actions in this controller
public class AdminController : Controller
{
    private readonly IProductService _productService;
    private readonly IProductServiceEF _productServiceEF;

    public AdminController(IProductService productService, IProductServiceEF productServiceEF)
    {
        _productService = productService;
        _productServiceEF = productServiceEF;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeViewModel();
        
        // Get products using both approaches for admin comparison
        viewModel.RawSqlProducts = await _productService.GetAllProductsAsync();
        viewModel.RawSqlCount = viewModel.RawSqlProducts.Count;
        
        viewModel.EntityFrameworkProducts = await _productServiceEF.GetAllProductsAsync();
        viewModel.EntityFrameworkCount = viewModel.EntityFrameworkProducts.Count;
        
        ViewBag.Message = "Welcome to the Admin Dashboard! This section is protected and requires JWT authentication.";
        ViewBag.Username = User.Identity?.Name ?? "Authenticated User";
        
        return View(viewModel);
    }
}
