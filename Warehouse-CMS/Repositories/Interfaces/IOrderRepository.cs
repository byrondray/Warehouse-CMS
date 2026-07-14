using Warehouse_CMS.Models;
using Warehouse_CMS.ViewModels;

namespace Warehouse_CMS.Repositories
{
    public interface IOrderRepository : IRepository<Order>, IAsyncRepository<Order>
    {
        /// <summary>
        /// One page of orders for the list view: includes only Customer and OrderStatus
        /// (what the list renders), not the full item/product graph, newest first.
        /// </summary>
        Task<PagedResult<Order>> GetPagedForListAsync(int pageNumber, int pageSize);
    }
}
