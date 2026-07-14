using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface IOrderRepository : IRepository<Order>, IAsyncRepository<Order>
    {
        /// <summary>
        /// Lightweight projection for the orders list: includes only Customer and OrderStatus
        /// (what the list view renders), not the full item/product graph.
        /// </summary>
        Task<IEnumerable<Order>> GetAllForListAsync();
    }
}
