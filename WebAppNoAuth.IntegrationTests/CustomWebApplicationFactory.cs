using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace WebAppNoAuth.IntegrationTests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        // Load testing-specific configuration
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.Testing.json", optional: false, reloadOnChange: true);
            // Enable environment variable expansion
            config.AddEnvironmentVariables();
        });
        
        builder.ConfigureServices(services =>
        {
            // Debug: Log the actual connection string being used
            var serviceProvider = services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();
            var connectionString = configuration?.GetConnectionString("DefaultConnection");
            Console.WriteLine($"DEBUG: Connection string from config: '{connectionString}'");
            
            // Override services if needed for testing
            // For example, you could replace database services with in-memory versions
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Configure host settings for testing
        builder.UseEnvironment("Testing");
        return base.CreateHost(builder);
    }
}
