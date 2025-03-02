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

        public OrderController(
            IOrderRepository orderRepository,
            IProductRepository productRepository
        )
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public IActionResult Index()
        {
            var orders = _orderRepository.GetAll().ToList();
            return View(orders);
        }

        public IActionResult Create()
        {
            var order = new Order
            {
                OrderDate = DateTime.Now,
                Status = "Pending",
                OrderItems = new List<OrderItem> { new OrderItem { Quantity = 1 } },
            };

            ViewBag.Products = _productRepository.GetAll();
            return View(order);
        }

        [HttpPost]
        public IActionResult Create(
            string CustomerName,
            List<int> productIds,
            List<int> quantities,
            string action,
            int? removeIndex
        )
        {
            System.Diagnostics.Debug.WriteLine($"Action: {action}, RemoveIndex: {removeIndex}");
            System.Diagnostics.Debug.WriteLine($"Customer: {CustomerName}");
            System.Diagnostics.Debug.WriteLine(
                $"Products: {productIds?.Count ?? 0}, Quantities: {quantities?.Count ?? 0}"
            );

            var order = new Order
            {
                CustomerName = CustomerName,
                OrderDate = DateTime.Now,
                Status = "Pending",
                OrderItems = new List<OrderItem>(),
            };

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
                return View(order);
            }

            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                ModelState.AddModelError("CustomerName", "Customer name is required");
                ViewBag.Products = _productRepository.GetAll();
                return View(order);
            }

            if (!order.OrderItems.Any() || order.OrderItems.Any(i => i.ProductId <= 0))
            {
                ModelState.AddModelError("", "You must select a product for each item");
                ViewBag.Products = _productRepository.GetAll();
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
                    return View(order);
                }

                if (product.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError("", $"Insufficient stock for product: {product.Name}");
                    ViewBag.Products = _productRepository.GetAll();
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
