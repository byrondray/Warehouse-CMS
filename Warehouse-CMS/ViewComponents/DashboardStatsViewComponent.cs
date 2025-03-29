using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;

namespace Warehouse_CMS.ViewComponents
{
    public class DashboardStatsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public DashboardStatsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new DashboardStatsViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                LowStockCount = await _context.Products.CountAsync(p => p.StockQuantity < 10),
                ActiveOrders = await _context.Orders.CountAsync(o => o.OrderStatusId != 4),
                SupplierCount = await _context.Suppliers.CountAsync(),
            };

            return View(viewModel);
        }
    }

    public class DashboardStatsViewModel
    {
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int ActiveOrders { get; set; }
        public int SupplierCount { get; set; }
    }
}
