using Microsoft.AspNetCore.Mvc;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;

public class OrderController : Controller
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderController(IOrderRepository orderRepository, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public IActionResult Index()
    {
        var orders = _orderRepository.GetAll();
        return View(orders);
    }

    public IActionResult Create()
    {
        var order = new Order
        {
            OrderDate = DateTime.Now,
            Status = "Pending",
            OrderItems = new List<OrderItem>(),
        };
        ViewBag.Products = _productRepository.GetAll();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Order order)
    {
        if (ModelState.IsValid)
        {
            foreach (var item in order.OrderItems)
            {
                var product = _productRepository.GetById(item.ProductId);
                if (product != null)
                {
                    if (product.StockQuantity >= item.Quantity)
                    {
                        product.StockQuantity -= item.Quantity;
                        _productRepository.Update(product);

                        item.UnitPrice = product.Price;
                    }
                    else
                    {
                        ModelState.AddModelError(
                            "",
                            $"Insufficient stock for product: {product.Name}"
                        );
                        ViewBag.Products = _productRepository.GetAll();
                        return View(order);
                    }
                }
            }

            order.TotalAmount = order.OrderItems.Sum(item => item.Quantity * item.UnitPrice);

            _orderRepository.Add(order);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Products = _productRepository.GetAll();
        return View(order);
    }

    public IActionResult Details(int id)
    {
        var order = _orderRepository.GetById(id);
        if (order == null)
        {
            return NotFound();
        }
        return View(order);
    }
}
