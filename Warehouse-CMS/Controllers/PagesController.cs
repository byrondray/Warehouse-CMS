using Microsoft.AspNetCore.Mvc;
using Warehouse_CMS.Repositories;
using Warehouse_CMS.ViewModels;

namespace Warehouse_CMS.Controllers
{
    public class PagesController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISupplierRepository _supplierRepository;

        public PagesController(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            ICategoryRepository categoryRepository,
            ISupplierRepository supplierRepository
        )
        {
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _categoryRepository = categoryRepository;
            _supplierRepository = supplierRepository;
        }

        public IActionResult About()
        {
            return PartialView();
        }

        public IActionResult Contact()
        {
            return PartialView();
        }

        public IActionResult Products()
        {
            var products = _productRepository
                .GetAll()
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier?.Name,
                });

            return PartialView(products);
        }

        public IActionResult Orders()
        {
            var orders = _orderRepository.GetAll().ToList();
            return PartialView(orders);
        }
    }
}
