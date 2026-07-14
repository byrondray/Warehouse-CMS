using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;
using Warehouse_CMS.ViewModels;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfProductRepository : EfCoreRepository<Product>, IProductRepository
    {
        public EfProductRepository(ApplicationDbContext context)
            : base(context) { }

        public async Task<PagedResult<ProductViewModel>> GetPagedAsync(
            int pageNumber,
            int pageSize
        )
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 20;

            var baseQuery = _dbSet.AsNoTracking().OrderBy(p => p.Name);
            var totalCount = await baseQuery.CountAsync();

            // Project to the view model in the query so only the needed columns are read;
            // no entity graph or OrderItems are materialized.
            var items = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    SupplierId = p.SupplierId,
                    SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                })
                .ToListAsync();

            return new PagedResult<ProductViewModel>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }

        // OrderItems are intentionally not included: the product list/details views only
        // use Category/Supplier, and a product's order history grows unboundedly.
        private IQueryable<Product> WithListIncludes() =>
            _dbSet.Include(p => p.Category).Include(p => p.Supplier);

        public override IEnumerable<Product> GetAll()
        {
            return WithListIncludes().ToList();
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await WithListIncludes().ToListAsync();
        }

        public IEnumerable<Product> GetLowStock(int threshold)
        {
            return _dbSet
                .Where(p => p.StockQuantity < threshold)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToList();
        }

        public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold)
        {
            return await _dbSet
                .Where(p => p.StockQuantity < threshold)
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .ToListAsync();
        }

        public override Product? GetById(int id)
        {
            return WithListIncludes().FirstOrDefault(p => p.Id == id);
        }

        public override async Task<Product?> GetByIdAsync(int id)
        {
            return await WithListIncludes().FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
