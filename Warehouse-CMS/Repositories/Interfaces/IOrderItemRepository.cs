using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface IOrderItemRepository : IRepository<OrderItem>, IAsyncRepository<OrderItem>
    {
        IEnumerable<OrderItem> GetByOrderId(int orderId);
    }
}
