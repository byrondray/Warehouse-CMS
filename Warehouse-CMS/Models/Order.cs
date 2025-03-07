using System.ComponentModel.DataAnnotations;

namespace Warehouse_CMS.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        public int OrderStatusId { get; set; }
        public OrderStatus? OrderStatus { get; set; }

        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
