using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;

namespace Warehouse_CMS.ViewComponents
{
    public class DashboardStatsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private const int LowStockThreshold = 5;

        public DashboardStatsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var inactiveStatusIds = await _context
                .OrderStatuses.Where(s =>
                    s.Status.ToLower() == "completed" || s.Status.ToLower() == "cancelled"
                )
                .Select(s => s.Id)
                .ToListAsync();

            var viewModel = new DashboardStatsViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                LowStockCount = await _context.Products.CountAsync(p =>
                    p.StockQuantity < LowStockThreshold
                ),
                ActiveOrders = await _context.Orders.CountAsync(o =>
                    !inactiveStatusIds.Contains(o.OrderStatusId)
                ),
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
