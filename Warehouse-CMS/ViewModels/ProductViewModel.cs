using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Warehouse_CMS.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a positive number")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public SelectList CategoryList { get; set; } =
            new SelectList(Enumerable.Empty<SelectListItem>());

        [Required(ErrorMessage = "Please select a supplier")]
        public int SupplierId { get; set; }

        public string SupplierName { get; set; } = string.Empty;

        public SelectList SupplierList { get; set; } =
            new SelectList(Enumerable.Empty<SelectListItem>());

        public bool IsInStock => StockQuantity > 0;
    }
}
