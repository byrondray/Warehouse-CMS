using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;

namespace Warehouse_CMS.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderStatusRepository _orderStatusRepository;

        public OrderController(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            IOrderStatusRepository orderStatusRepository
        )
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _orderStatusRepository = orderStatusRepository;
        }

        public IActionResult Index()
        {
            var orders = _orderRepository.GetAll().ToList();
            return View(orders);
        }

        public IActionResult Create()
        {
            // Find or create a "Pending" status
            var pendingStatus = _orderStatusRepository
                .GetAll()
                .FirstOrDefault(s => s.Status == "Pending");
            if (pendingStatus == null)
            {
                pendingStatus = new OrderStatus { Status = "Pending" };
                _orderStatusRepository.Add(pendingStatus);
            }

            var order = new Order
            {
                OrderDate = DateTime.Now,
                OrderStatusId = pendingStatus.Id,
                OrderStatus = pendingStatus,
                OrderItems = new List<OrderItem> { new OrderItem { Quantity = 1 } },
            };

            ViewBag.Products = _productRepository.GetAll();
            ViewBag.Customers = _customerRepository.GetAll();
            return View(order);
        }

        [HttpPost]
        public IActionResult Create(
            int CustomerId,
            string CustomerName, // For backward compatibility
            List<int> productIds,
            List<int> quantities,
            string action,
            int? removeIndex
        )
        {
            System.Diagnostics.Debug.WriteLine($"Action: {action}, RemoveIndex: {removeIndex}");
            System.Diagnostics.Debug.WriteLine(
                $"Customer ID: {CustomerId}, Customer Name: {CustomerName}"
            );
            System.Diagnostics.Debug.WriteLine(
                $"Products: {productIds?.Count ?? 0}, Quantities: {quantities?.Count ?? 0}"
            );

            // Find or create a "Pending" status
            var pendingStatus = _orderStatusRepository
                .GetAll()
                .FirstOrDefault(s => s.Status == "Pending");
            if (pendingStatus == null)
            {
                pendingStatus = new OrderStatus { Status = "Pending" };
                _orderStatusRepository.Add(pendingStatus);
            }

            var order = new Order
            {
                OrderDate = DateTime.Now,
                OrderStatusId = pendingStatus.Id,
                OrderStatus = pendingStatus,
                OrderItems = new List<OrderItem>(),
            };

            // Handle customer - either use CustomerId if provided or look up/create by name
            if (CustomerId > 0)
            {
                order.CustomerId = CustomerId;
                order.Customer = _customerRepository.GetById(CustomerId);
            }
            else if (!string.IsNullOrWhiteSpace(CustomerName))
            {
                // Find or create a customer with this name
                var customer = _customerRepository
                    .GetAll()
                    .FirstOrDefault(c => c.Name == CustomerName);
                if (customer == null)
                {
                    customer = new Customer { Name = CustomerName, CreatedAt = DateTime.Now };
                    _customerRepository.Add(customer);
                }
                order.CustomerId = customer.Id;
                order.Customer = customer;
            }

            if (productIds != null && quantities != null)
            {
                for (int i = 0; i < Math.Min(productIds.Count, quantities.Count); i++)
                {
                    if (productIds[i] > 0)
                    {
                        order.OrderItems.Add(
                            new OrderItem { ProductId = productIds[i], Quantity = quantities[i] }
                        );
                    }
                }
            }

            foreach (var item in order.OrderItems)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Item - ProductId: {item.ProductId}, Quantity: {item.Quantity}"
                );
            }

            if (action == "addItem")
            {
                order.OrderItems.Add(new OrderItem { Quantity = 1 });
                ViewBag.Products = _productRepository.GetAll();
                ViewBag.Customers = _customerRepository.GetAll();
                return View(order);
            }

            if (
                removeIndex.HasValue
                && removeIndex.Value >= 0
                && removeIndex.Value < order.OrderItems.Count
            )
            {
                order.OrderItems.RemoveAt(removeIndex.Value);

                if (order.OrderItems.Count == 0)
                {
                    order.OrderItems.Add(new OrderItem { Quantity = 1 });
                }

                ViewBag.Products = _productRepository.GetAll();
                ViewBag.Customers = _customerRepository.GetAll();
                return View(order);
            }

            if (order.CustomerId == 0)
            {
                ModelState.AddModelError("CustomerId", "Customer is required");
                ViewBag.Products = _productRepository.GetAll();
                ViewBag.Customers = _customerRepository.GetAll();
                return View(order);
            }

            if (!order.OrderItems.Any() || order.OrderItems.Any(i => i.ProductId <= 0))
            {
                ModelState.AddModelError("", "You must select a product for each item");
                ViewBag.Products = _productRepository.GetAll();
                ViewBag.Customers = _customerRepository.GetAll();
                return View(order);
            }

            decimal totalAmount = 0;

            foreach (var item in order.OrderItems)
            {
                var product = _productRepository.GetById(item.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError("", $"Product with ID {item.ProductId} not found");
                    ViewBag.Products = _productRepository.GetAll();
                    ViewBag.Customers = _customerRepository.GetAll();
                    return View(order);
                }

                if (product.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError("", $"Insufficient stock for product: {product.Name}");
                    ViewBag.Products = _productRepository.GetAll();
                    ViewBag.Customers = _customerRepository.GetAll();
                    return View(order);
                }

                product.StockQuantity -= item.Quantity;
                _productRepository.Update(product);

                item.UnitPrice = product.Price;
                totalAmount += item.Quantity * item.UnitPrice;
            }

            order.TotalAmount = totalAmount;

            _orderRepository.Add(order);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            var order = _orderRepository.GetById(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.OrderItems != null)
            {
                foreach (var item in order.OrderItems)
                {
                    item.Product = _productRepository.GetById(item.ProductId);
                }
            }

            return View(order);
        }
    }
}
