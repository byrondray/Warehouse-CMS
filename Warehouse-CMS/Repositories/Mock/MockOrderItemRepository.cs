using System;
using System.Collections.Generic;
using System.Linq;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public class MockOrderItemRepository : IOrderItemRepository
    {
        private static List<OrderItem> _orderItems;
        private readonly IProductRepository _productRepository;

        public MockOrderItemRepository(IProductRepository productRepository)
        {
            _productRepository = productRepository;

            if (_orderItems == null)
            {
                var product1 = _productRepository.GetById(1);
                var product2 = _productRepository.GetById(2);

                _orderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = 1,
                        OrderId = 1,
                        ProductId = product1.Id,
                        Product = product1,
                        Quantity = 10,
                        UnitPrice = product1.Price,
                    },
                    new OrderItem
                    {
                        Id = 2,
                        OrderId = 1,
                        ProductId = product2.Id,
                        Product = product2,
                        Quantity = 5,
                        UnitPrice = product2.Price,
                    },
                    new OrderItem
                    {
                        Id = 3,
                        OrderId = 2,
                        ProductId = product2.Id,
                        Product = product2,
                        Quantity = 20,
                        UnitPrice = product2.Price,
                    },
                };
            }
        }

        public IEnumerable<OrderItem> GetAll()
        {
            System.Diagnostics.Debug.WriteLine(
                $"Getting all order items. Count: {_orderItems.Count}"
            );
            return _orderItems;
        }

        public OrderItem GetById(int id)
        {
            var orderItem = _orderItems.FirstOrDefault(o => o.Id == id);
            System.Diagnostics.Debug.WriteLine(
                $"Getting order item by id {id}: {(orderItem != null ? "Found" : "Not found")}"
            );
            return orderItem;
        }

        public IEnumerable<OrderItem> GetByOrderId(int orderId)
        {
            var items = _orderItems.Where(o => o.OrderId == orderId).ToList();
            System.Diagnostics.Debug.WriteLine(
                $"Getting order items by order id {orderId}: Found {items.Count} items"
            );
            return items;
        }

        public void Add(OrderItem orderItem)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Adding order item for product {orderItem.ProductId}, quantity: {orderItem.Quantity}"
            );
            orderItem.Id = _orderItems.Any() ? _orderItems.Max(o => o.Id) + 1 : 1;
            orderItem.Product = _productRepository.GetById(orderItem.ProductId);
            _orderItems.Add(orderItem);
            System.Diagnostics.Debug.WriteLine(
                $"Order item added. Total items: {_orderItems.Count}"
            );
        }

        public void Update(OrderItem orderItem)
        {
            System.Diagnostics.Debug.WriteLine($"Updating order item: {orderItem.Id}");
            var existing = _orderItems.FirstOrDefault(o => o.Id == orderItem.Id);
            if (existing != null)
            {
                orderItem.Product = _productRepository.GetById(orderItem.ProductId);
                var index = _orderItems.IndexOf(existing);
                _orderItems[index] = orderItem;
                System.Diagnostics.Debug.WriteLine($"Order item updated successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Order item with ID {orderItem.Id} not found for update"
                );
            }
        }

        public void Delete(int id)
        {
            System.Diagnostics.Debug.WriteLine($"Deleting order item: {id}");
            var orderItem = _orderItems.FirstOrDefault(o => o.Id == id);
            if (orderItem != null)
            {
                _orderItems.Remove(orderItem);
                System.Diagnostics.Debug.WriteLine(
                    $"Order item deleted. Remaining items: {_orderItems.Count}"
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Order item with ID {id} not found for deletion"
                );
            }
        }
    }
}
