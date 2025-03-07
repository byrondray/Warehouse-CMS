using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

public static class SeedDatabase
{
    public static void Seed(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

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

            var employeeRoles = new List<EmployeeRole>
            {
                new EmployeeRole
                {
                    Role = "Sales Associate",
                    Description = "Handles customer orders and sales",
                },
                new EmployeeRole
                {
                    Role = "Manager",
                    Description = "Oversees warehouse operations",
                },
                new EmployeeRole
                {
                    Role = "Warehouse Staff",
                    Description = "Handles inventory and shipping",
                },
            };
            dbContext.EmployeeRoles.AddRange(employeeRoles);
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
    }
}
