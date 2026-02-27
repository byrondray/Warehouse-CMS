using System.ComponentModel.DataAnnotations;

namespace Warehouse_CMS.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        public int OrderStatusId { get; set; }
        public OrderStatus? OrderStatus { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
