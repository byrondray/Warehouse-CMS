using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface IOrderStatusRepository : IRepository<OrderStatus>, IAsyncRepository<OrderStatus> { }
}
