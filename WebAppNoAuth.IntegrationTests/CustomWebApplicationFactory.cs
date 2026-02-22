using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace WebAppNoAuth.IntegrationTests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
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
