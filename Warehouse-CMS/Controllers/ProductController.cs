using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Warehouse_CMS.Repositories;
using Warehouse_CMS.ViewModels;

public class ProductController : Controller
{
    private readonly IProductRepository _repository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductController(IProductRepository repository, ICategoryRepository categoryRepository)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
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
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name,
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
        ViewBag.CategoryName = product.Category?.Name;

        var viewModel = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        var viewModel = new ProductViewModel
        {
            CategoryList = new SelectList(_categoryRepository.GetAll(), "Id", "Name"),
        };
        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Create(ProductViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var product = new Product
            {
                Name = viewModel.Name,
                Description = viewModel.Description,
                Price = viewModel.Price,
                StockQuantity = viewModel.StockQuantity,
                CategoryId = viewModel.CategoryId,
            };

            _repository.Add(product);
            return RedirectToAction(nameof(Index));
        }

        viewModel.CategoryList = new SelectList(
            _categoryRepository.GetAll(),
            "Id",
            "Name",
            viewModel.CategoryId
        );
        return View(viewModel);
    }

    public IActionResult Edit(int id)
    {
        var product = _repository.GetById(id);
        if (product == null)
        {
            return NotFound();
        }

        var viewModel = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            CategoryList = new SelectList(
                _categoryRepository.GetAll(),
                "Id",
                "Name",
                product.CategoryId
            ),
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Edit(int id, ProductViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var product = new Product
            {
                Id = viewModel.Id,
                Name = viewModel.Name,
                Description = viewModel.Description,
                Price = viewModel.Price,
                StockQuantity = viewModel.StockQuantity,
                CategoryId = viewModel.CategoryId,
            };

            _repository.Update(product);
            return RedirectToAction(nameof(Index));
        }

        viewModel.CategoryList = new SelectList(
            _categoryRepository.GetAll(),
            "Id",
            "Name",
            viewModel.CategoryId
        );
        return View(viewModel);
    }

    public IActionResult Delete(int id)
    {
        var product = _repository.GetById(id);
        if (product == null)
        {
            return NotFound();
        }

        var viewModel = new ProductViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        _repository.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}
