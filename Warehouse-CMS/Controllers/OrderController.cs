using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;
using Warehouse_CMS.ViewModels;

namespace Warehouse_CMS.Controllers
{
    [Authorize]
    public class OrderController : BaseController
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeIdentityRepository _employeeIdentityRepository;
        private readonly IInventoryService _inventoryService;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            IOrderStatusRepository orderStatusRepository,
            IEmployeeRepository employeeRepository,
            IEmployeeIdentityRepository employeeIdentityRepository,
            IInventoryService inventoryService,
            ApplicationDbContext dbContext,
            UserManager<IdentityUser> userManager,
            ILogger<OrderController> logger
        )
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _orderStatusRepository = orderStatusRepository;
            _employeeRepository = employeeRepository;
            _employeeIdentityRepository = employeeIdentityRepository;
            _inventoryService = inventoryService;
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
        }

        // The order create/edit form always needs the product and customer pick lists;
        // populate them in one place so the many re-display paths stay in sync.
        private async Task PopulateOrderFormListsAsync()
        {
            ViewBag.Products = await _productRepository.GetAllAsync();
            ViewBag.Customers = await _customerRepository.GetAllAsync();
        }

        private async Task<OrderStatus> GetOrCreatePendingStatusAsync()
        {
            var pendingStatus = (await _orderStatusRepository.GetAllAsync()).FirstOrDefault(s =>
                s.Status == "Pending"
            );
            if (pendingStatus == null)
            {
                pendingStatus = new OrderStatus { Status = "Pending" };
                await _orderStatusRepository.AddAsync(pendingStatus);
            }

            return pendingStatus;
        }

        private const int PageSize = 20;

        [Authorize(Roles = "Admin,Manager,Sales Associate")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var orders = await _orderRepository.GetPagedForListAsync(page, PageSize);

            return ViewOrPartial("_OrdersList", orders);
        }

        [Authorize(Roles = "Admin,Manager,Sales Associate")]
        public async Task<IActionResult> Create()
        {
            var pendingStatus = await GetOrCreatePendingStatusAsync();

            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                OrderStatusId = pendingStatus.Id,
                OrderStatus = pendingStatus,
                OrderItems = new List<OrderItem> { new OrderItem { Quantity = 1 } },
            };

            await PopulateOrderFormListsAsync();

            return ViewOrPartial("_CreateOrder", order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Sales Associate")]
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
                await PopulateOrderFormListsAsync();
                return View(
                    new Order
                    {
                        OrderDate = DateTime.UtcNow,
                        OrderItems = new List<OrderItem> { new OrderItem { Quantity = 1 } },
                    }
                );
            }

            var pendingStatus = await GetOrCreatePendingStatusAsync();

            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                OrderStatusId = pendingStatus.Id,
                OrderStatus = pendingStatus,
                OrderItems = new List<OrderItem>(),
                EmployeeId = employee.Id,
                Employee = employee,
            };

            Customer? customer = null;
            if (CustomerId > 0)
            {
                customer = await _customerRepository.GetByIdAsync(CustomerId);
            }
            else if (!string.IsNullOrWhiteSpace(CustomerName))
            {
                customer = new Customer { Name = CustomerName, CreatedAt = DateTime.UtcNow };
                await _customerRepository.AddAsync(customer);
            }

            if (customer == null)
            {
                ModelState.AddModelError("", "A customer must be selected or created");
                await PopulateOrderFormListsAsync();
                return View(order);
            }

            order.CustomerId = customer.Id;
            order.Customer = customer;

            if (action == "addItem")
            {
                order.OrderItems.Add(new OrderItem { Quantity = 1 });
                await PopulateOrderFormListsAsync();
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

                await PopulateOrderFormListsAsync();
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
                await PopulateOrderFormListsAsync();
                return View(order);
            }

            if (order.OrderItems.Any(i => i.Quantity < 1))
            {
                ModelState.AddModelError("", "Each item must have a quantity of at least 1");
                await PopulateOrderFormListsAsync();
                return View(order);
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                decimal totalAmount = 0;
                foreach (var item in order.OrderItems)
                {
                    var stockError = await _inventoryService.DeductStockForOrderItemAsync(item);
                    if (stockError != null)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", stockError);
                        await PopulateOrderFormListsAsync();
                        return View(order);
                    }

                    totalAmount += item.Quantity * item.UnitPrice;
                }

                order.TotalAmount = totalAmount;
                await _orderRepository.AddAsync(order);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "An error occurred while creating an order.");
                ModelState.AddModelError(
                    "",
                    "An error occurred while creating the order. Please try again."
                );
                await PopulateOrderFormListsAsync();
                return View(order);
            }

            return JsonOrRedirect(nameof(Index));
        }

        [Authorize(Roles = "Admin,Manager,Sales Associate")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Sales Associate")]
        public async Task<IActionResult> Edit(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var viewModel = new OrderEditViewModel
            {
                Id = order.Id,
                OrderStatusId = order.OrderStatusId,
            };

            ViewBag.OrderStatuses = await _orderStatusRepository.GetAllAsync();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager,Sales Associate")]
        public async Task<IActionResult> Edit(OrderEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var originalOrder = await _orderRepository.GetByIdAsync(model.Id);
                if (originalOrder == null)
                {
                    return NotFound();
                }

                originalOrder.OrderStatusId = model.OrderStatusId;

                await _orderRepository.UpdateAsync(originalOrder);

                return RedirectToAction(nameof(Index));
            }

            ViewBag.OrderStatuses = await _orderStatusRepository.GetAllAsync();
            return View(model);
        }
    }
}
