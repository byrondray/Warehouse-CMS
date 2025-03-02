using System;
using System.Collections.Generic;
using System.Linq;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public class MockOrderRepository : IOrderRepository
    {
        private static List<Order> _orders;

        public MockOrderRepository()
        {
            if (_orders == null)
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

            // Debug - print out orders at initialization
            System.Diagnostics.Debug.WriteLine(
                $"Repository initialized with {_orders.Count} orders"
            );
            foreach (var order in _orders)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Order: {order.Id}, Customer: {order.CustomerName}, Total: {order.TotalAmount}"
                );
            }
        }

        public IEnumerable<Order> GetAll()
        {
            System.Diagnostics.Debug.WriteLine(
                $"GetAll() called, returning {_orders.Count} orders"
            );
            return _orders;
        }

        public Order GetById(int id)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            System.Diagnostics.Debug.WriteLine(
                $"GetById({id}) called, {(order != null ? "found" : "not found")}"
            );
            return order;
        }

        public void Add(Order order)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Add() called with CustomerName: {order.CustomerName}, Items: {order.OrderItems?.Count ?? 0}"
            );

            try
            {
                order.Id = _orders.Any() ? _orders.Max(o => o.Id) + 1 : 1;

                // Ensure OrderItems is not null
                if (order.OrderItems == null)
                {
                    order.OrderItems = new List<OrderItem>();
                    System.Diagnostics.Debug.WriteLine(
                        "Warning: OrderItems was null, initialized empty list"
                    );
                }

                // Set OrderId on each OrderItem
                foreach (var item in order.OrderItems)
                {
                    item.OrderId = order.Id;
                    System.Diagnostics.Debug.WriteLine(
                        $"  Item: ProductId={item.ProductId}, Quantity={item.Quantity}, UnitPrice={item.UnitPrice}"
                    );
                }

                _orders.Add(order);
                System.Diagnostics.Debug.WriteLine(
                    $"Order added successfully. New count: {_orders.Count}"
                );

                // Verify after adding
                var addedOrder = _orders.FirstOrDefault(o => o.Id == order.Id);
                if (addedOrder != null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Verification: Order {addedOrder.Id} exists in repository"
                    );
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        "ERROR: Failed to find added order in repository after add!"
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in Add(): {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                throw; // Re-throw to maintain original exception
            }
        }

        public void Update(Order order)
        {
            System.Diagnostics.Debug.WriteLine($"Update() called for OrderId: {order.Id}");

            try
            {
                var existing = _orders.FirstOrDefault(o => o.Id == order.Id);
                if (existing != null)
                {
                    var index = _orders.IndexOf(existing);
                    _orders[index] = order;
                    System.Diagnostics.Debug.WriteLine($"Order {order.Id} updated successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"ERROR: Order {order.Id} not found for update"
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in Update(): {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                throw; // Re-throw to maintain original exception
            }
        }

        public void Delete(int id)
        {
            System.Diagnostics.Debug.WriteLine($"Delete() called for OrderId: {id}");

            try
            {
                var order = _orders.FirstOrDefault(o => o.Id == id);
                if (order != null)
                {
                    _orders.Remove(order);
                    System.Diagnostics.Debug.WriteLine(
                        $"Order {id} deleted successfully. Remaining orders: {_orders.Count}"
                    );
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: Order {id} not found for deletion");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in Delete(): {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                throw; // Re-throw to maintain original exception
            }
        }
    }
}
