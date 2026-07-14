using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfSupplierRepository : EfCoreRepository<Supplier>, ISupplierRepository
    {
        public EfSupplierRepository(ApplicationDbContext context)
            : base(context) { }

        public override IEnumerable<Supplier> GetAll()
        {
            return _dbSet.Include(s => s.Products).ToList();
        }

        public override async Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return await _dbSet.Include(s => s.Products).ToListAsync();
        }

        public override Supplier? GetById(int id)
        {
            return _dbSet.Include(s => s.Products).FirstOrDefault(s => s.Id == id);
        }

        public override async Task<Supplier?> GetByIdAsync(int id)
        {
            return await _dbSet.Include(s => s.Products).FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
