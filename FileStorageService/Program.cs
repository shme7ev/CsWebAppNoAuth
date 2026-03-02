using FileStorageService.Data;
using FileStorageService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "File Storage Service API", 
        Version = "v1",
        Description = "REST API for storing, retrieving, and managing binary files with metadata"
    });
    
    c.CustomOperationIds(apiDesc => apiDesc.HttpMethod + apiDesc.RelativePath);
    
    // Add XML comments if available
    // var xmlFile = "FileStorageService.xml";
    // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    // if (File.Exists(xmlPath))
    // {
    //     c.IncludeXmlComments(xmlPath);
    // }
});

builder.Services.AddDbContext<FileStorageDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFileStorageService, FileStorageServiceImpl>();

// Configure file storage path
builder.Configuration.AddEnvironmentVariables();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// // Ensure database is created and migrations are applied - needed?
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<FileStorageDbContext>();
//     try
//     {
//         context.Database.EnsureCreated();
//         // Or use migrations if you prefer:
//         // context.Database.Migrate();
//     }
//     catch (Exception ex)
//     {
//         var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
//         logger.LogError(ex, "An error occurred while creating the database.");
//     }
// }

app.Run();
