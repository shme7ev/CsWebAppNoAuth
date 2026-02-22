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

    public IActionResult Dashboard()
    {
        ViewBag.Username = User.Identity?.Name ?? "Authenticated User";
        ViewBag.Message = "Admin Dashboard - Protected Area";
        
        return View();
    }

    [HttpGet]
    public IActionResult GenerateToken()
    {
        // This action allows generating tokens for testing purposes
        // In a real application, this would be secured or removed
        return View();
    }

    [HttpPost]
    public IActionResult GenerateToken(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            ViewBag.Error = "Username is required";
            return View();
        }

        // In a real application, you would validate credentials here
        // For demo purposes, we'll generate a token for any username
        
        var tokenService = HttpContext.RequestServices.GetService<IJwtTokenService>();
        if (tokenService == null)
        {
            ViewBag.Error = "Token service not available";
            return View();
        }

        var token = tokenService.GenerateToken(username);
        ViewBag.Token = token;
        ViewBag.Username = username;
        ViewBag.Success = "Token generated successfully!";
        
        return View();
    }

    [AllowAnonymous] // Allow anonymous access to the token generation endpoint
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [AllowAnonymous] // Allow anonymous access to the token generation endpoint
    [HttpPost]
    public IActionResult Login(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            ViewBag.Error = "Username is required";
            return View();
        }

        var tokenService = HttpContext.RequestServices.GetService<IJwtTokenService>();
        if (tokenService == null)
        {
            ViewBag.Error = "Token service not available";
            return View();
        }

        var token = tokenService.GenerateToken(username);
        ViewBag.Token = token;
        ViewBag.Username = username;
        ViewBag.Success = "Login successful! Your JWT token is ready.";
        
        return View();
    }
}