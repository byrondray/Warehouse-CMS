using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;
using Warehouse_CMS.Repositories.Implementation;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configure MySQL with Pomelo provider
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    )
);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Explicitly disable Identity services
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.IUserStore<Microsoft.AspNetCore.Identity.IdentityUser>>(
    provider => null
);
builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser>>(
    provider => null
);

builder.Services.AddRazorPages(options =>
{
    // Disable authentication requirements for Razor Pages
    options.Conventions.AllowAnonymousToPage("/");
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

// Add controllers
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created
        dbContext.Database.EnsureCreated();

        // Seed data
        SeedDatabase.Seed(services);
    }
    catch (Exception ex)
    {
        // Log the error
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Remove or comment out authentication middleware
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "products",
    pattern: "products/{action=Index}/{id?}",
    defaults: new { controller = "Product" }
);

// Ensure Razor Pages are mapped
app.MapRazorPages();

// Ensure the database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
