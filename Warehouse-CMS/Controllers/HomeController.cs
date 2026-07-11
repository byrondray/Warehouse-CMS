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

        private const int LOW_STOCK_THRESHOLD = 5;

        public HomeController(
            ILogger<HomeController> logger,
            IInventoryService inventoryService
        )
        {
            _logger = logger;
            _inventoryService = inventoryService;
        }

        public IActionResult Index(bool route = false)
        {
            // Dashboard stat tiles (product/order/supplier counts) are rendered by the
            // DashboardStats view component; the view only needs the low-stock product list here.
            ViewBag.LowStockProducts = _inventoryService.GetLowStockProducts(LOW_STOCK_THRESHOLD);

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
