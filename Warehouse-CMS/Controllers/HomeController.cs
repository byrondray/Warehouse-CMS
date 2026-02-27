using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;

namespace Warehouse_CMS.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IInventoryService _inventoryService;
        private readonly ApplicationDbContext _dbContext;

        private const int LOW_STOCK_THRESHOLD = 5;

        public HomeController(
            ILogger<HomeController> logger,
            IInventoryService inventoryService,
            ApplicationDbContext dbContext
        )
        {
            _logger = logger;
            _inventoryService = inventoryService;
            _dbContext = dbContext;
        }

        public IActionResult Index(bool route = false)
        {
            ViewBag.TotalProducts = _dbContext.Products.Count();

            var lowStockProducts = _inventoryService.GetLowStockProducts(LOW_STOCK_THRESHOLD);
            ViewBag.LowStockCount = lowStockProducts.Count;
            ViewBag.LowStockProducts = lowStockProducts;

            var completedStatusIds = _dbContext
                .OrderStatuses.Where(s =>
                    s.Status.ToLower() == "completed" || s.Status.ToLower() == "cancelled"
                )
                .Select(s => s.Id)
                .ToList();

            ViewBag.ActiveOrders = _dbContext.Orders.Count(o =>
                !completedStatusIds.Contains(o.OrderStatusId)
            );

            ViewBag.SupplierCount = _dbContext.Suppliers.Count();

            if (route)
            {
                return PartialView();
            }

            if (IsAjaxRequest())
            {
                return PartialView("_Index");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            if (IsAjaxRequest())
            {
                return PartialView("_Privacy");
            }
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
    }
}
