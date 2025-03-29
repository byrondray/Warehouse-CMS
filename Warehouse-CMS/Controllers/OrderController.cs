using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;

namespace Warehouse_CMS.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeIdentityRepository _employeeIdentityRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            IOrderStatusRepository orderStatusRepository,
            IEmployeeRepository employeeRepository,
            IEmployeeIdentityRepository employeeIdentityRepository,
            UserManager<IdentityUser> userManager
        )
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _orderStatusRepository = orderStatusRepository;
            _employeeRepository = employeeRepository;
            _employeeIdentityRepository = employeeIdentityRepository;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var orders = _orderRepository.GetAll().ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_OrdersList", orders);
            }

            return View(orders);
        }

        public IActionResult Create()
        {
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
        public async Task<IActionResult> Create(
            int CustomerId,
            string CustomerName,
            List<int> productIds,
            List<int> quantities,
            string action,
            int? removeIndex
        )
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var employee = await _employeeIdentityRepository.GetEmployeeByIdentityUserIdAsync(
                currentUser.Id
            );
            if (employee == null)
            {
                ModelState.AddModelError(
                    "",
                    "Your user account is not linked to an employee record"
                );
                ViewBag.Products = _productRepository.GetAll();
                ViewBag.Customers = _customerRepository.GetAll();
                return View(
                    new Order
                    {
                        OrderDate = DateTime.Now,
                        OrderItems = new List<OrderItem> { new OrderItem { Quantity = 1 } },
                    }
                );
            }

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
                EmployeeId = employee.Id,
                Employee = employee,
            };

            Customer customer = null;
            if (CustomerId > 0)
            {
                customer = _customerRepository.GetById(CustomerId);
            }
            else if (!string.IsNullOrWhiteSpace(CustomerName))
            {
                customer = new Customer { Name = CustomerName, CreatedAt = DateTime.Now };
                _customerRepository.Add(customer);
            }

            if (customer == null)
            {
                ModelState.AddModelError("", "A customer must be selected or created");
                ViewBag.Products = _productRepository.GetAll();
                ViewBag.Customers = _customerRepository.GetAll();
                return View(order);
            }

            order.CustomerId = customer.Id;
            order.Customer = customer;

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

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var order = _orderRepository.GetById(id);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.OrderStatuses = _orderStatusRepository.GetAll();
            return View(order);
        }

        [HttpPost]
        public IActionResult Edit(Order order)
        {
            if (ModelState.IsValid)
            {
                var originalOrder = _orderRepository.GetById(order.Id);
                if (originalOrder == null)
                {
                    return NotFound();
                }

                originalOrder.OrderStatusId = order.OrderStatusId;

                _orderRepository.Update(originalOrder);

                return RedirectToAction(nameof(Index));
            }

            ViewBag.OrderStatuses = _orderStatusRepository.GetAll();
            return View(order);
        }
    }
}
