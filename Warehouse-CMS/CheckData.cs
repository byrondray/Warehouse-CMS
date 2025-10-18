using System;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;

namespace Warehouse_CMS
{
    public static class CheckData
    {
        public static void Main(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("DATABASE_URL not found");
                return;
            }

            var uri = new Uri(connectionString);
            var pgConnectionString =
                $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(pgConnectionString);

            using var context = new ApplicationDbContext(optionsBuilder.Options);

            Console.WriteLine($"Categories: {context.Categories.Count()}");
            Console.WriteLine($"Products: {context.Products.Count()}");
            Console.WriteLine($"Customers: {context.Customers.Count()}");
            Console.WriteLine($"Employees: {context.Employees.Count()}");
            Console.WriteLine($"Suppliers: {context.Suppliers.Count()}");
            Console.WriteLine($"OrderStatuses: {context.OrderStatuses.Count()}");
            Console.WriteLine($"Users: {context.Users.Count()}");
            Console.WriteLine($"Roles: {context.Roles.Count()}");
        }
    }
}
