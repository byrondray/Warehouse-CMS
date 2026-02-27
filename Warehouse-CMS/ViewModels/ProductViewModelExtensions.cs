using Warehouse_CMS.Models;

namespace Warehouse_CMS.ViewModels
{
    public static class ProductViewModelExtensions
    {
        public static ProductViewModel ToViewModel(this Product product)
        {
            return new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name,
            };
        }
    }
}
