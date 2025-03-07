using System.ComponentModel.DataAnnotations;

namespace Warehouse_CMS.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
