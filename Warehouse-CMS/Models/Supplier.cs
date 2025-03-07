using System.ComponentModel.DataAnnotations;

namespace Warehouse_CMS.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string ContactPerson { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new List<Product>();
    }
}
