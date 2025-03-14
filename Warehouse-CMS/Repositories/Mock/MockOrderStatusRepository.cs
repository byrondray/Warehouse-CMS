using System.Collections.Generic;
using System.Linq;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public class MockOrderStatusRepository : IOrderStatusRepository
    {
        private static List<OrderStatus> _orderStatuses;

        public MockOrderStatusRepository()
        {
            if (_orderStatuses == null)
            {
                _orderStatuses = new List<OrderStatus>
                {
                    new OrderStatus
                    {
                        Id = 1,
                        Status = "Completed",
                        Orders = new List<Order>(),
                    },
                    new OrderStatus
                    {
                        Id = 2,
                        Status = "Processing",
                        Orders = new List<Order>(),
                    },
                    new OrderStatus
                    {
                        Id = 3,
                        Status = "Pending",
                        Orders = new List<Order>(),
                    },
                    new OrderStatus
                    {
                        Id = 4,
                        Status = "Cancelled",
                        Orders = new List<Order>(),
                    },
                };
            }
        }

        public IEnumerable<OrderStatus> GetAll()
        {
            System.Diagnostics.Debug.WriteLine(
                $"Getting all order statuses. Count: {_orderStatuses.Count}"
            );
            return _orderStatuses;
        }

        public OrderStatus GetById(int id)
        {
            var status = _orderStatuses.FirstOrDefault(s => s.Id == id);
            System.Diagnostics.Debug.WriteLine(
                $"Getting order status by id {id}: {(status != null ? status.Status : "Not found")}"
            );
            return status;
        }

        public void Add(OrderStatus orderStatus)
        {
            System.Diagnostics.Debug.WriteLine($"Adding order status: {orderStatus.Status}");
            orderStatus.Id = _orderStatuses.Any() ? _orderStatuses.Max(s => s.Id) + 1 : 1;
            _orderStatuses.Add(orderStatus);
            System.Diagnostics.Debug.WriteLine(
                $"Order status added. Total statuses: {_orderStatuses.Count}"
            );
        }

        public void Update(OrderStatus orderStatus)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Updating order status: {orderStatus.Id} - {orderStatus.Status}"
            );
            var existing = _orderStatuses.FirstOrDefault(s => s.Id == orderStatus.Id);
            if (existing != null)
            {
                var index = _orderStatuses.IndexOf(existing);
                _orderStatuses[index] = orderStatus;
                System.Diagnostics.Debug.WriteLine($"Order status updated successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Order status with ID {orderStatus.Id} not found for update"
                );
            }
        }

        public void Delete(int id)
        {
            System.Diagnostics.Debug.WriteLine($"Deleting order status: {id}");
            var status = _orderStatuses.FirstOrDefault(s => s.Id == id);
            if (status != null)
            {
                _orderStatuses.Remove(status);
                System.Diagnostics.Debug.WriteLine(
                    $"Order status deleted. Remaining statuses: {_orderStatuses.Count}"
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Order status with ID {id} not found for deletion"
                );
            }
        }
    }
}
