using Microsoft.AspNetCore.Identity;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

public static class SeedDatabase
{
    public static void Seed(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        EnsureEmployeeRolesExist(dbContext);

        if (!dbContext.Categories.Any())
        {
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Construction Materials",
                    Description = "Building and construction materials",
                },
                new Category { Name = "Tools", Description = "Construction and building tools" },
                new Category
                {
                    Name = "Safety Equipment",
                    Description = "Protective gear and safety supplies",
                },
            };
            dbContext.Categories.AddRange(categories);
            dbContext.SaveChanges();

            var orderStatuses = new List<OrderStatus>
            {
                new OrderStatus { Status = "Completed" },
                new OrderStatus { Status = "Processing" },
                new OrderStatus { Status = "Pending" },
                new OrderStatus { Status = "Cancelled" },
            };
            dbContext.OrderStatuses.AddRange(orderStatuses);
            dbContext.SaveChanges();

            var suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Name = "Home Depot",
                    ContactPerson = "Mike Johnson",
                    Email = "mike@techsupplies.com",
                    Phone = "555-0123",
                },
                new Supplier
                {
                    Name = "Lowes",
                    ContactPerson = "Sarah Williams",
                    Email = "sarah@officefurniture.com",
                    Phone = "555-0456",
                },
            };
            dbContext.Suppliers.AddRange(suppliers);
            dbContext.SaveChanges();

            var customers = new List<Customer>
            {
                new Customer { Name = "John Doe", CreatedAt = DateTime.Now.AddDays(-30) },
                new Customer { Name = "Jane Smith", CreatedAt = DateTime.Now.AddDays(-15) },
                new Customer { Name = "Bob Johnson", CreatedAt = DateTime.Now.AddDays(-5) },
            };
            dbContext.Customers.AddRange(customers);
            dbContext.SaveChanges();

            var employees = new List<Employee>
            {
                new Employee
                {
                    Name = "Alice Brown",
                    StartDate = DateTime.Now.AddYears(-2),
                    EmployeeRoleId = 1,
                },
                new Employee
                {
                    Name = "Charlie Davis",
                    StartDate = DateTime.Now.AddYears(-5),
                    EmployeeRoleId = 2,
                },
            };
            dbContext.Employees.AddRange(employees);
            dbContext.SaveChanges();

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Drywall Sheet",
                    Description = "4' x 8' standard drywall sheet, 1/2\" thickness",
                    Price = 12.99m,
                    StockQuantity = 250,
                    CategoryId = 1,
                    SupplierId = 1,
                },
                new Product
                {
                    Name = "Hammer",
                    Description = "16 oz. claw hammer with fiberglass handle",
                    Price = 14.99m,
                    StockQuantity = 75,
                    CategoryId = 2,
                    SupplierId = 2,
                },
                new Product
                {
                    Name = "Concrete Mix",
                    Description = "60 lb. ready-to-use concrete mix",
                    Price = 6.50m,
                    StockQuantity = 320,
                    CategoryId = 1,
                    SupplierId = 1,
                },
            };
            dbContext.Products.AddRange(products);
            dbContext.SaveChanges();
        }

        SeedIdentityRoles(dbContext, roleManager).Wait();
        SeedAdminUser(userManager, roleManager).Wait();
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
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        var adminEmail = "admin@warehouse.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var user = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
            };

            var password = "Admin@123456";

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}
