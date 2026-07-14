using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;
using Warehouse_CMS.Repositories.Implementation;
using Warehouse_CMS.Services;
using Warehouse_CMS.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Pin the app-wide culture so currency/number formatting (ToString("C")) is
// consistent regardless of the host/container locale (e.g. Alpine on Railway).
var defaultCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"Current environment: {environment}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    var connectionString = !string.IsNullOrEmpty(databaseUrl)
        ? BuildNpgsqlConnectionString(databaseUrl)
        : builder.Configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException(
            "No database connection configured. Set the DATABASE_URL environment variable, "
                + "or a 'DefaultConnection' connection string for local development."
        );
    }

    options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure());

    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    {
        options.EnableSensitiveDataLogging();
    }
});

// Parse a Railway/Heroku-style postgres:// URL into an Npgsql connection string.
// Uri.UserInfo is percent-encoded, so credentials must be unescaped; the password
// is optional and may itself contain ':'.
static string BuildNpgsqlConnectionString(string databaseUrl)
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;

    var csb = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = username,
        Password = password,
        SslMode = Npgsql.SslMode.Require,
    };

    return csb.ConnectionString;
}

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
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
        }
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

var authBuilder = builder.Services.AddAuthentication();

var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.CallbackPath = "/signin-google";
    });
}

// Configure external authentication cookies
builder.Services.ConfigureExternalCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(15); // Give enough time for user to fill form
    options.SlidingExpiration = false; // Don't extend on activity, user should complete registration

    if (!builder.Environment.IsDevelopment())
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.HttpOnly = true;
    }
    else
    {
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    }
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
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    }
    else if (builder.Environment.IsStaging())
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.HttpOnly = true;
    }
    else
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.HttpOnly = true;
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

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

// Health check that actually verifies the database is reachable, so /health reflects
// real readiness (a liveness-only stub would report healthy even with Postgres down).
builder
    .Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(name: "database");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    // Migration failures leave the app in a broken state (every request would error at
    // runtime), so fail fast and let the platform's restart policy retry rather than
    // starting up against an unmigrated database.
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    // Seeding is best-effort: a seed failure (e.g. ADMIN_PASSWORD unset) should be logged
    // loudly but must not take down an otherwise-healthy app.
    try
    {
        await SeedDatabase.SeedAsync(services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
    app.UseStatusCodePages();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseStatusCodePagesWithReExecute("/Environment/StatusCode", "?statusCode={0}");
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

app.MapHealthChecks("/health").AllowAnonymous();

app.MapRazorPages();

app.Run();
