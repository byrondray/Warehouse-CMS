using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public class MockOrderRepository : IOrderRepository
    {
        private List<Order> _orders;

        public MockOrderRepository()
        {
            _orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    OrderDate = DateTime.Now.AddDays(-5),
                    CustomerName = "John Doe",
                    Status = "Completed",
                    OrderItems = new List<OrderItem>(),
                    TotalAmount = 1499.99m,
                },
                new Order
                {
                    Id = 2,
                    OrderDate = DateTime.Now.AddDays(-2),
                    CustomerName = "Jane Smith",
                    Status = "Processing",
                    OrderItems = new List<OrderItem>(),
                    TotalAmount = 299.99m,
                },
            };
        }

        public IEnumerable<Order> GetAll() => _orders;

        public Order GetById(int id) => _orders.FirstOrDefault(o => o.Id == id);

        public void Add(Order order)
        {
            order.Id = _orders.Max(o => o.Id) + 1;
            _orders.Add(order);
        }

        public void Update(Order order)
        {
            var existing = _orders.FirstOrDefault(o => o.Id == order.Id);
            if (existing != null)
            {
                var index = _orders.IndexOf(existing);
                _orders[index] = order;
            }
        }

        public void Delete(int id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                _orders.Remove(order);
            }
        }
    }
}
