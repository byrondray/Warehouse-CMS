using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;
using Warehouse_CMS.Repositories.Implementation;
using Warehouse_CMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Output current environment name for debugging
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"Current environment: {environment}");

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure database with environment-specific settings
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    {
        // In development/testing, enable detailed errors and sensitive data logging
        options
            .UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure()
            )
            .EnableSensitiveDataLogging();
    }
    else
    {
        // In staging/production, disable detailed errors for security
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure()
        );
    }
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Register email sender BEFORE Identity
builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

// Configure and register application settings from appsettings.json
builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("ApplicationSettings")
);

// Identity configuration with environment-specific options
builder
    .Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Base settings
        options.SignIn.RequireConfirmedAccount = false;

        // Environment-specific password requirements
        if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
        {
            // Simpler passwords for development/testing
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 4;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        }
        else
        {
            // Stricter passwords for staging/production
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
        }
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Cookie configuration with environment-specific timeouts
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;

    // Environment-specific cookie settings
    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    {
        // Short expiration for testing
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    }
    else if (builder.Environment.IsStaging())
    {
        // Medium expiration for staging
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    }
    else
    {
        // Longer expiration for production
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    }
});

// Repository registrations
builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
builder.Services.AddScoped<ICustomerRepository, EfCustomerRepository>();
builder.Services.AddScoped<IEmployeeRepository, EfEmployeeRepository>();
builder.Services.AddScoped<IEmployeeRoleRepository, EfEmployeeRoleRepository>();
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, EfOrderItemRepository>();
builder.Services.AddScoped<IOrderStatusRepository, EfOrderStatusRepository>();
builder.Services.AddScoped<IProductRepository, EfProductRepository>();
builder.Services.AddScoped<ISupplierRepository, EfSupplierRepository>();
builder.Services.AddScoped<IInventoryService, InventoryService>();

// Add controllers and Razor pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.EnsureCreated();

        SeedDatabase.Seed(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline with environment-specific middleware
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    // Development/Testing specific error handling
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else if (app.Environment.IsStaging())
{
    // Staging might use a custom error page with some details
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Production uses the most generic error page
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Route configuration
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "products",
    pattern: "products/{action=Index}/{id?}",
    defaults: new { controller = "Product" }
);

// Add a route for environment demonstration
app.MapControllerRoute(
    name: "environment",
    pattern: "environment/{action=Index}/{id?}",
    defaults: new { controller = "Environment" }
);

app.MapRazorPages();

app.Run();
