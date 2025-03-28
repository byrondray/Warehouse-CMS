using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;
using Warehouse_CMS.ViewModels;

namespace Warehouse_CMS.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductRepository _repository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ISupplierRepository _supplierRepository;

        public ProductController(
            IProductRepository repository,
            ICategoryRepository categoryRepository,
            ISupplierRepository supplierRepository
        )
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
            _supplierRepository = supplierRepository;
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
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier?.Name,
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
            ViewBag.Category = product.Category?.Name;
            ViewBag.Supplier = product.Supplier?.Name;

            var viewModel = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name,
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            var viewModel = new ProductViewModel
            {
                CategoryList = new SelectList(_categoryRepository.GetAll(), "Id", "Name"),
                SupplierList = new SelectList(_supplierRepository.GetAll(), "Id", "Name"),
            };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
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
                    SupplierId = viewModel.SupplierId,
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
            viewModel.SupplierList = new SelectList(
                _supplierRepository.GetAll(),
                "Id",
                "Name",
                viewModel.SupplierId
            );
            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Manager")]
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
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name,
                CategoryList = new SelectList(
                    _categoryRepository.GetAll(),
                    "Id",
                    "Name",
                    product.CategoryId
                ),
                SupplierList = new SelectList(
                    _supplierRepository.GetAll(),
                    "Id",
                    "Name",
                    product.SupplierId
                ),
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
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
                    SupplierId = viewModel.SupplierId,
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
            viewModel.SupplierList = new SelectList(
                _supplierRepository.GetAll(),
                "Id",
                "Name",
                viewModel.SupplierId
            );
            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Manager")]
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
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name,
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.Delete(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
