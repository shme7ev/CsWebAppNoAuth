using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate:
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}"
        )
        .WriteTo.File("logs/webapp-.log",
            outputTemplate:
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7)
        .CreateLogger()
);

builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen();

// Add session services
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // Add custom event to enrich claims with user roles
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userService = context.HttpContext.RequestServices
                    .GetRequiredService<WebAppNoAuth.Services.IUserService>();
                var username = context.Principal?.Identity?.Name;

                if (!string.IsNullOrEmpty(username))
                {
                    var user = await userService.GetUserByUsernameAsync(username);
                    if (user != null)
                    {
                        var claims = new List<System.Security.Claims.Claim>
                        {
                            new(System.Security.Claims.ClaimTypes.Role, user.Role)
                        };

                        var appIdentity = new System.Security.Claims.ClaimsIdentity(claims);
                        context.Principal?.AddIdentity(appIdentity);
                    }
                }
            }
        };
    });

// Register database service (raw SQL)
builder.Services.AddScoped<WebAppNoAuth.Services.IProductService, WebAppNoAuth.Services.ProductService>();

// Register Entity Framework services
builder.Services.AddDbContext<WebAppNoAuth.Data.ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register EF-based product service
builder.Services.AddScoped<WebAppNoAuth.Services.IProductServiceEF, WebAppNoAuth.Services.ProductServiceEF>();

// Register JWT token service
builder.Services.AddScoped<WebAppNoAuth.Services.IJwtTokenService, WebAppNoAuth.Services.JwtTokenService>();

// Register User service
builder.Services.AddScoped<WebAppNoAuth.Services.IUserService, WebAppNoAuth.Services.UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

//add token to request header.
app.Use(async (HttpContext context, RequestDelegate next) =>
{
    var token = context.Session.GetString("Token");
    if (!string.IsNullOrEmpty(token)) context.Request.Headers.Append("Authorization", "Bearer " + token);
    await next(context);
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
