using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAppNoAuth.Models;
using WebAppNoAuth.Services;
using Microsoft.Extensions.Logging;

namespace WebAppNoAuth.Controllers;

[Authorize] // This attribute requires JWT authentication for all actions in this controller
public class AdminController(
    IProductService productService,
    IProductServiceEF productServiceEf,
    IUserService userService,
    ILogger<AdminController> logger)
    : Controller
{
    private async Task<string> GetUserRoleAsync()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return "Unknown";

        var user = await userService.GetUserByUsernameAsync(username);
        logger.LogDebug("Retrieved role for user {Username}: {Role}", username, user?.Role ?? "Unknown");
        return user?.Role ?? "Unknown";
    }

    public async Task<IActionResult> Index()
    {
        logger.LogDebug("AdminController.Index() called");
        var username = User.Identity?.Name ?? "Unknown";
        logger.LogDebug("Admin dashboard accessed by user: {Username}", username);
        
        var viewModel = new HomeViewModel();

        logger.LogDebug("Fetching products for admin dashboard");
        viewModel.RawSqlProducts = await productService.GetAllProductsAsync();
        viewModel.RawSqlCount = viewModel.RawSqlProducts.Count;

        viewModel.EntityFrameworkProducts = await productServiceEf.GetAllProductsAsync();
        viewModel.EntityFrameworkCount = viewModel.EntityFrameworkProducts.Count;

        ViewBag.Message = "Welcome to the Admin Dashboard! This section is protected and requires JWT authentication.";
        ViewBag.Username = username;
        ViewBag.UserRole = await GetUserRoleAsync();

        logger.LogDebug("Admin dashboard loaded with {RawSqlCount} (SQL) and {EntityFrameworkCount} (EF) products for user: {Username}",
                             viewModel.RawSqlCount, viewModel.EntityFrameworkCount, username);
        return View(viewModel);
    }

    // Method accessible only to users with Admin role
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UserManager()
    {
        logger.LogDebug("AdminController.UserManager() called");
        var username = User.Identity?.Name ?? "Unknown";
        logger.LogDebug("User management accessed by admin user: {Username}", username);
        
        var users = await userService.GetAllUsersAsync();
        ViewBag.Message = "User Management - Admin Only";
        ViewBag.Username = username;
        ViewBag.UserRole = await GetUserRoleAsync();

        logger.LogDebug("Loaded {UserCount} users for management by admin: {Username}", users.Count, username);
        return View("UserManager", users);
    }

    // Method accessible to Admin and Manager roles
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reports()
    {
        logger.LogDebug("AdminController.Reports() called");
        var username = User.Identity?.Name ?? "Unknown";
        var userRole = await GetUserRoleAsync();
        logger.LogDebug("Reports dashboard accessed by user: {Username}, Role: {UserRole}", username, userRole);
        
        var users = await userService.GetAllUsersAsync();
        var userStats = new
        {
            TotalUsers = users.Count,
            AdminUsers = users.Count(u => u.Role == "Admin"),
            ManagerUsers = users.Count(u => u.Role == "Manager"),
            RegularUsers = users.Count(u => u.Role == "User" || u.Role == "Developer")
        };

        ViewBag.Message = "Reports Dashboard - Admin and Managers Only";
        ViewBag.Username = username;
        ViewBag.UserRole = userRole;
        ViewBag.Stats = userStats;

        logger.LogDebug("Reports generated - Total: {TotalUsers}, Admins: {AdminUsers}, Managers: {ManagerUsers}, Regular: {RegularUsers}",
                             userStats.TotalUsers, userStats.AdminUsers, userStats.ManagerUsers, userStats.RegularUsers);
        return View("Reports");
    }

    // Method accessible to all authenticated users
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        logger.LogDebug("AdminController.Profile() called");
        var username = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(username))
        {
            logger.LogWarning("Profile access attempted without username, redirecting to index");
            return RedirectToAction("Index");
        }

        logger.LogDebug("Loading profile for user: {Username}", username);
        var user = await userService.GetUserByUsernameAsync(username);
        ViewBag.Message = "User Profile";
        ViewBag.Username = username;
        ViewBag.UserRole = await GetUserRoleAsync();

        logger.LogDebug("Profile loaded for user: {Username}, Role: {Role}", username, user?.Role ?? "Unknown");
        return View("Profile", user);
    }

    // Method for testing different role access levels
    [HttpGet]
    [AllowAnonymous] // This allows access without authentication for testing purposes
    public IActionResult Login()
    {
        logger.LogDebug("AdminController.Login() called (AllowAnonymous)");
        logger.LogDebug("Admin login page accessed");
        return View();
    }
}
