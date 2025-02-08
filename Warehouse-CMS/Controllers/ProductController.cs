using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;
using Warehouse_CMS.ViewModels;

public class ProductController : Controller
{
    private readonly IProductRepository _repository;

    public ProductController(IProductRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        var products = _repository
            .GetAll()
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Category = p.Category,
            });

        return View(products);
    }

    public IActionResult Details(int id)
    {
        var product = _repository.GetById(id);
        if (product == null)
        {
            return NotFound();
        }

        ViewData["PageTitle"] = $"Product Details - {product.Name}";
        ViewBag.Category = product.Category;

        var viewModel = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _repository.Add(product);
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }
}
