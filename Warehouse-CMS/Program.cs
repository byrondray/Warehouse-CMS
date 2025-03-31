using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Warehouse_CMS.Attributes;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;
using Warehouse_CMS.Repositories.Implementation;
using Warehouse_CMS.Services;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"Current environment: {environment}");

var connectionString =
    builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING")
    ?? throw new InvalidOperationException(
        "Connection string 'AZURE_SQL_CONNECTIONSTRING' not found."
    );

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    // {
    //     options
    //         .UseMySql(
    //             connectionString,
    //             ServerVersion.AutoDetect(connectionString),
    //             mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    //         )
    //         .EnableSensitiveDataLogging();
    // }
    // else
    // {
    //     options.UseMySql(
    //         connectionString,
    //         ServerVersion.AutoDetect(connectionString),
    //         mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    //     );
    // }

    options.UseSqlServer(connectionString);
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection("ApplicationSettings")
);

builder
    .Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 4;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        }
        else
        {
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

builder
    .Services.AddAuthentication()
    .AddGoogle(options =>
    {
        IConfiguration configuration = builder.Configuration;
        options.ClientId = configuration["Authentication:Google:ClientId"];
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/signin-google";
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;

    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    }
    else if (builder.Environment.IsStaging())
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
    }
    else
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    }
});

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
builder.Services.AddScoped<IRoleManagementRepository, RoleManagementRepository>();
builder.Services.AddScoped<IEmployeeIdentityRepository, EmployeeIdentityRepository>();

builder.Services.AddScoped<IRoleManagementService, RoleManagementService>();

// Update the MVC service registration to include the SPA filters
builder.Services.AddControllersWithViews(options =>
{
    // Register the SPA action filter
    options.Filters.Add<SpaActionFilter>();
});

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
    }
}

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

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
    app.UseStatusCodePages();
}
else if (app.Environment.IsStaging())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?statusCode={0}");
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?statusCode={0}");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use(
    async (context, next) =>
    {
        var path = context.Request.Path.Value;
        if (path != null && path.EndsWith(".map"))
        {
            context.Response.StatusCode = 404;
            return;
        }
        await next();
    }
);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "products",
    pattern: "products/{action=Index}/{id?}",
    defaults: new { controller = "Product" }
);

app.MapControllerRoute(
    name: "environment",
    pattern: "environment/{action=Index}/{id?}",
    defaults: new { controller = "Environment" }
);

app.MapFallbackToController("Index", "Home");

app.MapRazorPages();

app.Run();
