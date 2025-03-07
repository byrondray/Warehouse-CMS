using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure one-to-many relationship between Category and Product
            modelBuilder
                .Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Configure one-to-many relationship between Supplier and Product
            modelBuilder
                .Entity<Product>()
                .HasOne(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierId);

            // Configure one-to-many relationship between EmployeeRole and Employee
            modelBuilder
                .Entity<Employee>()
                .HasOne(e => e.EmployeeRole)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.EmployeeRoleId);

            // Configure one-to-many relationship between Customer and Order
            modelBuilder
                .Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);

            // Configure one-to-many relationship between Employee and Order
            modelBuilder
                .Entity<Order>()
                .HasOne(o => o.Employee)
                .WithMany(e => e.Orders)
                .HasForeignKey(o => o.EmployeeId);

            // Configure one-to-many relationship between OrderStatus and Order
            modelBuilder
                .Entity<Order>()
                .HasOne(o => o.OrderStatus)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.OrderStatusId);

            // Configure one-to-many relationship between Order and OrderItem
            modelBuilder
                .Entity<OrderItem>()
                .HasOne(i => i.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(i => i.OrderId);

            // Configure one-to-many relationship between Product and OrderItem
            modelBuilder
                .Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(i => i.ProductId);
        }
    }
}
