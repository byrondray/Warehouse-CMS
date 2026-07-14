using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;
using Warehouse_CMS.ViewModels;

namespace Warehouse_CMS.Controllers
{
    [Authorize]
    public class ProductController : BaseController
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

        private const int PageSize = 20;

        public async Task<IActionResult> Index(int page = 1)
        {
            var products = await _repository.GetPagedAsync(page, PageSize);

            if (IsAjaxRequest())
            {
                return PartialView("_ProductsList", products);
            }

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["PageTitle"] = $"Product Details - {product.Name}";
            ViewBag.Category = product.Category?.Name;
            ViewBag.Supplier = product.Supplier?.Name;

            var viewModel = product.ToViewModel();

            if (IsAjaxRequest())
            {
                return PartialView("_ProductDetails", viewModel);
            }

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            var viewModel = new ProductViewModel
            {
                CategoryList = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name"),
                SupplierList = new SelectList(await _supplierRepository.GetAllAsync(), "Id", "Name"),
            };

            return ViewOrPartial("_CreateProduct", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(ProductViewModel viewModel)
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

                await _repository.AddAsync(product);

                return JsonOrRedirect(nameof(Index));
            }

            viewModel.CategoryList = new SelectList(
                await _categoryRepository.GetAllAsync(),
                "Id",
                "Name",
                viewModel.CategoryId
            );
            viewModel.SupplierList = new SelectList(
                await _supplierRepository.GetAllAsync(),
                "Id",
                "Name",
                viewModel.SupplierId
            );

            return ViewOrPartial("_CreateProduct", viewModel);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var viewModel = product.ToViewModel();
            viewModel.CategoryList = new SelectList(
                await _categoryRepository.GetAllAsync(),
                "Id",
                "Name",
                product.CategoryId
            );
            viewModel.SupplierList = new SelectList(
                await _supplierRepository.GetAllAsync(),
                "Id",
                "Name",
                product.SupplierId
            );

            return ViewOrPartial("_EditProduct", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, ProductViewModel viewModel)
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

                await _repository.UpdateAsync(product);

                return JsonOrRedirect(nameof(Index));
            }

            viewModel.CategoryList = new SelectList(
                await _categoryRepository.GetAllAsync(),
                "Id",
                "Name",
                viewModel.CategoryId
            );
            viewModel.SupplierList = new SelectList(
                await _supplierRepository.GetAllAsync(),
                "Id",
                "Name",
                viewModel.SupplierId
            );

            return ViewOrPartial("_EditProduct", viewModel);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var viewModel = product.ToViewModel();

            return ViewOrPartial("_DeleteProduct", viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repository.DeleteAsync(id);

            return JsonOrRedirect(nameof(Index));
        }
    }
}
