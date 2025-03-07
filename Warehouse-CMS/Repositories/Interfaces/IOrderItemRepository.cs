using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface IOrderItemRepository
    {
        IEnumerable<OrderItem> GetAll();
        OrderItem GetById(int id);
        IEnumerable<OrderItem> GetByOrderId(int orderId);
        void Add(OrderItem orderItem);
        void Update(OrderItem orderItem);
        void Delete(int id);
    }
}
