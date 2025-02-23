using System.ComponentModel.DataAnnotations;

namespace Warehouse_CMS.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public List<Product> Products { get; set; } = new List<Product>();
    }
}
