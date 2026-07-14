using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfProductRepository : EfCoreRepository<Product>, IProductRepository
    {
        public EfProductRepository(ApplicationDbContext context)
            : base(context) { }

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
