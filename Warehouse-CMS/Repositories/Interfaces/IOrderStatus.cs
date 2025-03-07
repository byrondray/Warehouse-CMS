using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface IOrderStatusRepository
    {
        IEnumerable<OrderStatus> GetAll();
        OrderStatus GetById(int id);
        void Add(OrderStatus orderStatus);
        void Update(OrderStatus orderStatus);
        void Delete(int id);
    }
}
