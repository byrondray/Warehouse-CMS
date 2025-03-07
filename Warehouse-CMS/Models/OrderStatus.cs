using System.ComponentModel.DataAnnotations;

namespace Warehouse_CMS.Models
{
    public class OrderStatus
    {
        public int Id { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
