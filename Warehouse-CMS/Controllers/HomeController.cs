using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse_CMS.Repositories;

namespace Warehouse_CMS.Controllers
{
    [Authorize]
    [ActivatorUtilitiesConstructor]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderStatusRepository _orderStatusRepository;

        // private readonly IWebHostEnvironment _environment;

        private const int LOW_STOCK_THRESHOLD = 5;

        public HomeController(
            ILogger<HomeController> logger,
            IProductRepository productRepository,
            ISupplierRepository supplierRepository,
            IOrderRepository orderRepository,
            IOrderStatusRepository orderStatusRepository
        // IWebHostEnvironment environment
        )
        {
            _logger = logger;
            _productRepository = productRepository;
            _supplierRepository = supplierRepository;
            _orderRepository = orderRepository;
            _orderStatusRepository = orderStatusRepository;
            // _environment = environment;
        }

        public IActionResult Index()
        {
            var products = _productRepository.GetAll().ToList();

            var suppliers = _supplierRepository.GetAll().ToList();

            var orders = _orderRepository.GetAll().ToList();

            var orderStatuses = _orderStatusRepository.GetAll().ToList();

            ViewBag.TotalProducts = products.Count;

            var lowStockProducts = products
                .Where(p => p.StockQuantity < LOW_STOCK_THRESHOLD)
                .ToList();
            ViewBag.LowStockCount = lowStockProducts.Count;
            ViewBag.LowStockProducts = lowStockProducts;

            var completedStatusIds = orderStatuses
                .Where(s => s.Status.ToLower() == "completed" || s.Status.ToLower() == "cancelled")
                .Select(s => s.Id)
                .ToList();

            var activeOrders = orders
                .Where(o => !completedStatusIds.Contains(o.OrderStatusId))
                .ToList();
            ViewBag.ActiveOrders = activeOrders.Count;

            ViewBag.SupplierCount = suppliers.Count;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                }
            );
        }

        // public IActionResult StatusCode(int statusCode)
        // {
        //     if (statusCode == 404)
        //     {
        //         if (_environment.IsProduction() || _environment.IsStaging())
        //         {
        //             return View("NotFound");
        //         }
        //     }

        //     return View(
        //         "Error",
        //         new ErrorViewModel
        //         {
        //             RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
        //         }
        //     );
        // }
    }
}
