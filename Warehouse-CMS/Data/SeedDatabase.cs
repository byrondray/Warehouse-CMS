using Microsoft.AspNetCore.Identity;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

public static class SeedDatabase
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var environment = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

        EnsureEmployeeRolesExist(dbContext);

        if (!dbContext.Categories.Any())
        {
            // Relationships are wired via navigation properties, not hardcoded FK ids: the
            // database generates identity ids that are not guaranteed to be 1, 2, 3… on a
            // fresh database, so assigning CategoryId = 1 etc. would throw an FK violation.
            var construction = new Category
            {
                Name = "Construction Materials",
                Description = "Building and construction materials",
            };
            var tools = new Category
            {
                Name = "Tools",
                Description = "Construction and building tools",
            };
            var safety = new Category
            {
                Name = "Safety Equipment",
                Description = "Protective gear and safety supplies",
            };
            dbContext.Categories.AddRange(construction, tools, safety);

            var orderStatuses = new List<OrderStatus>
            {
                new OrderStatus { Status = "Completed" },
                new OrderStatus { Status = "Processing" },
                new OrderStatus { Status = "Pending" },
                new OrderStatus { Status = "Cancelled" },
            };
            dbContext.OrderStatuses.AddRange(orderStatuses);

            var homeDepot = new Supplier
            {
                Name = "Home Depot",
                ContactPerson = "Mike Johnson",
                Email = "mike@techsupplies.com",
                Phone = "555-0123",
            };
            var lowes = new Supplier
            {
                Name = "Lowes",
                ContactPerson = "Sarah Williams",
                Email = "sarah@officefurniture.com",
                Phone = "555-0456",
            };
            dbContext.Suppliers.AddRange(homeDepot, lowes);

            var customers = new List<Customer>
            {
                new Customer { Name = "John Doe", CreatedAt = DateTime.UtcNow.AddDays(-30) },
                new Customer { Name = "Jane Smith", CreatedAt = DateTime.UtcNow.AddDays(-15) },
                new Customer { Name = "Bob Johnson", CreatedAt = DateTime.UtcNow.AddDays(-5) },
            };
            dbContext.Customers.AddRange(customers);

            var adminRole = dbContext.EmployeeRoles.First(r => r.Role == "Admin");
            var salesRole = dbContext.EmployeeRoles.First(r => r.Role == "Sales Associate");

            var employees = new List<Employee>
            {
                new Employee
                {
                    Name = "Alice Brown",
                    StartDate = DateTime.UtcNow.AddYears(-2),
                    EmployeeRole = adminRole,
                },
                new Employee
                {
                    Name = "Charlie Davis",
                    StartDate = DateTime.UtcNow.AddYears(-5),
                    EmployeeRole = salesRole,
                },
            };
            dbContext.Employees.AddRange(employees);

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Drywall Sheet",
                    Description = "4' x 8' standard drywall sheet, 1/2\" thickness",
                    Price = 12.99m,
                    StockQuantity = 250,
                    Category = construction,
                    Supplier = homeDepot,
                },
                new Product
                {
                    Name = "Hammer",
                    Description = "16 oz. claw hammer with fiberglass handle",
                    Price = 14.99m,
                    StockQuantity = 75,
                    Category = tools,
                    Supplier = lowes,
                },
                new Product
                {
                    Name = "Concrete Mix",
                    Description = "60 lb. ready-to-use concrete mix",
                    Price = 6.50m,
                    StockQuantity = 320,
                    Category = construction,
                    Supplier = homeDepot,
                },
            };
            dbContext.Products.AddRange(products);

            dbContext.SaveChanges();
        }

        await SeedIdentityRoles(dbContext, roleManager);
        await SeedAdminUser(dbContext, userManager, roleManager, environment);
    }

    private static void EnsureEmployeeRolesExist(ApplicationDbContext context)
    {
        var requiredRoles = new List<(string Role, string Description)>
        {
            ("Admin", "System administrator with full access"),
            ("Sales Associate", "Handles customer orders and sales"),
            ("Manager", "Oversees warehouse operations"),
            ("Warehouse Staff", "Handles inventory and shipping"),
        };

        foreach (var roleInfo in requiredRoles)
        {
            if (!context.EmployeeRoles.Any(r => r.Role == roleInfo.Role))
            {
                context.EmployeeRoles.Add(
                    new EmployeeRole { Role = roleInfo.Role, Description = roleInfo.Description }
                );
            }
        }

        context.SaveChanges();
    }

    private static async Task SeedIdentityRoles(
        ApplicationDbContext context,
        RoleManager<IdentityRole> roleManager
    )
    {
        var employeeRoles = context.EmployeeRoles.ToList();

        foreach (var employeeRole in employeeRoles)
        {
            if (!await roleManager.RoleExistsAsync(employeeRole.Role))
            {
                await roleManager.CreateAsync(new IdentityRole(employeeRole.Role));
            }
        }
    }

    private static async Task SeedAdminUser(
        ApplicationDbContext dbContext,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment environment
    )
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@warehouse.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
            };

            var isDevLike =
                environment.IsDevelopment() || environment.IsEnvironment("Testing");
            var password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (string.IsNullOrEmpty(password))
            {
                if (!isDevLike)
                {
                    // Never create a live admin account with a well-known default password
                    // outside dev/test: once created, changing ADMIN_PASSWORD has no effect.
                    throw new InvalidOperationException(
                        "ADMIN_PASSWORD environment variable must be set to seed the admin user "
                            + $"in the {environment.EnvironmentName} environment."
                    );
                }

                password = "Admin@123456";
            }

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                adminUser = user;
            }
        }

        if (adminUser != null && !dbContext.Employees.Any(e => e.UserId == adminUser.Id))
        {
            var adminRole = dbContext.EmployeeRoles.FirstOrDefault(r => r.Role == "Admin");
            if (adminRole != null)
            {
                dbContext.Employees.Add(
                    new Employee
                    {
                        Name = "Admin",
                        StartDate = DateTime.UtcNow,
                        EmployeeRoleId = adminRole.Id,
                        UserId = adminUser.Id,
                    }
                );
                dbContext.SaveChanges();
            }
        }
    }
}
