using System;
using System.Collections.Generic;
using System.Linq;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public class MockOrderRepository : IOrderRepository
    {
        private static List<Order> _orders;
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IOrderStatusRepository _orderStatusRepository;

        public MockOrderRepository(
            ICustomerRepository customerRepository,
            IEmployeeRepository employeeRepository,
            IOrderStatusRepository orderStatusRepository
        )
        {
            _customerRepository = customerRepository;
            _employeeRepository = employeeRepository;
            _orderStatusRepository = orderStatusRepository;

            if (_orders == null)
            {
                var completedStatus = _orderStatusRepository.GetById(1); // Completed
                var processingStatus = _orderStatusRepository.GetById(2); // Processing
                var customer1 = _customerRepository.GetById(1); // John Doe
                var customer2 = _customerRepository.GetById(2); // Jane Smith
                var employee1 = _employeeRepository.GetById(1); // Sales Employee

                _orders = new List<Order>
                {
                    new Order
                    {
                        Id = 1,
                        OrderDate = DateTime.Now.AddDays(-5),
                        CustomerId = customer1.Id,
                        Customer = customer1,
                        EmployeeId = employee1.Id,
                        Employee = employee1,
                        OrderStatusId = completedStatus.Id,
                        OrderStatus = completedStatus,
                        OrderItems = new List<OrderItem>(),
                        TotalAmount = 1499.99m,
                    },
                    new Order
                    {
                        Id = 2,
                        OrderDate = DateTime.Now.AddDays(-2),
                        CustomerId = customer2.Id,
                        Customer = customer2,
                        EmployeeId = employee1.Id,
                        Employee = employee1,
                        OrderStatusId = processingStatus.Id,
                        OrderStatus = processingStatus,
                        OrderItems = new List<OrderItem>(),
                        TotalAmount = 299.99m,
                    },
                };
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
                $"Add() called for Order with {order.OrderItems?.Count ?? 0} items"
            );

            try
            {
                order.Id = _orders.Any() ? _orders.Max(o => o.Id) + 1 : 1;

                // Ensure OrderItems is not null
                if (order.OrderItems == null)
                {
                    order.OrderItems = new List<OrderItem>();
                }

                // Set OrderId on each OrderItem
                foreach (var item in order.OrderItems)
                {
                    item.OrderId = order.Id;
                }

                // Handle customer based on the controller's approach
                // For existing controllers that might be passing a "CustomerName" from a form
                // We'll extract it from the request and find/create a customer
                var customerNameFromRequest = GetCustomerNameFromRequest();
                if (!string.IsNullOrEmpty(customerNameFromRequest))
                {
                    // Find or create a customer with this name
                    var customer = _customerRepository
                        .GetAll()
                        .FirstOrDefault(c => c.Name == customerNameFromRequest);
                    if (customer == null)
                    {
                        customer = new Customer
                        {
                            Name = customerNameFromRequest,
                            CreatedAt = DateTime.Now,
                        };
                        _customerRepository.Add(customer);
                    }
                    order.CustomerId = customer.Id;
                    order.Customer = customer;
                }
                else if (order.CustomerId > 0 && order.Customer == null)
                {
                    // If CustomerId is set but Customer is not, load the Customer
                    order.Customer = _customerRepository.GetById(order.CustomerId);
                }
                else if (order.CustomerId == 0)
                {
                    // If no customer is specified, use a default one
                    var customer = _customerRepository.GetAll().FirstOrDefault();
                    if (customer != null)
                    {
                        order.CustomerId = customer.Id;
                        order.Customer = customer;
                    }
                }

                // Handle status based on the controller's approach
                // For existing controllers that might be passing a "Status" from a form
                var statusFromRequest = GetStatusFromRequest();
                if (!string.IsNullOrEmpty(statusFromRequest))
                {
                    // Find or create an order status with this name
                    var orderStatus = _orderStatusRepository
                        .GetAll()
                        .FirstOrDefault(s => s.Status == statusFromRequest);
                    if (orderStatus == null)
                    {
                        orderStatus = new OrderStatus { Status = statusFromRequest };
                        _orderStatusRepository.Add(orderStatus);
                    }
                    order.OrderStatusId = orderStatus.Id;
                    order.OrderStatus = orderStatus;
                }
                else if (order.OrderStatusId > 0 && order.OrderStatus == null)
                {
                    // If OrderStatusId is set but OrderStatus is not, load the OrderStatus
                    order.OrderStatus = _orderStatusRepository.GetById(order.OrderStatusId);
                }
                else if (order.OrderStatusId == 0)
                {
                    // If no status is specified, use "Pending" or the first available
                    var orderStatus =
                        _orderStatusRepository.GetAll().FirstOrDefault(s => s.Status == "Pending")
                        ?? _orderStatusRepository.GetAll().FirstOrDefault();
                    if (orderStatus != null)
                    {
                        order.OrderStatusId = orderStatus.Id;
                        order.OrderStatus = orderStatus;
                    }
                }

                // If no employee assigned, use the first employee
                if (order.EmployeeId == 0)
                {
                    var employee = _employeeRepository.GetAll().FirstOrDefault();
                    if (employee != null)
                    {
                        order.EmployeeId = employee.Id;
                        order.Employee = employee;
                    }
                }
                else if (order.Employee == null && order.EmployeeId > 0)
                {
                    order.Employee = _employeeRepository.GetById(order.EmployeeId);
                }

                _orders.Add(order);
                System.Diagnostics.Debug.WriteLine(
                    $"Order added successfully. New count: {_orders.Count}"
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR in Add(): {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                throw;
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
                    // Get related entities if not already set
                    if (order.Customer == null && order.CustomerId > 0)
                    {
                        order.Customer = _customerRepository.GetById(order.CustomerId);
                    }

                    if (order.Employee == null && order.EmployeeId > 0)
                    {
                        order.Employee = _employeeRepository.GetById(order.EmployeeId);
                    }

                    if (order.OrderStatus == null && order.OrderStatusId > 0)
                    {
                        order.OrderStatus = _orderStatusRepository.GetById(order.OrderStatusId);
                    }

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
                throw;
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
                throw;
            }
        }

        // Helper methods to handle data from HTML forms in case your controller is still
        // expecting CustomerName and Status as form fields
        private string GetCustomerNameFromRequest()
        {
            // In a real implementation, you might access these from
            // HttpContext.Current.Request.Form["CustomerName"]
            // But for this mock repository, we'll return null
            return null;
        }

        private string GetStatusFromRequest()
        {
            // In a real implementation, you might access these from
            // HttpContext.Current.Request.Form["Status"]
            // But for this mock repository, we'll return null
            return null;
        }
    }
}
