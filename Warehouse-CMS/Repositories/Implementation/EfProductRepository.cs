using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfProductRepository : EfCoreRepository<Product>, IProductRepository
    {
        public EfProductRepository(ApplicationDbContext context)
            : base(context) { }

        public override IEnumerable<Product> GetAll()
        {
            return _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.OrderItems)
                .ToList();
        }

        public override Product GetById(int id)
        {
            return _dbSet
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Include(p => p.OrderItems)
                .FirstOrDefault(p => p.Id == id);
        }
    }
}
