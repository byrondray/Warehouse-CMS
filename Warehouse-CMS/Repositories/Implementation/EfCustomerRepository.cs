using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfCustomerRepository : EfCoreRepository<Customer>, ICustomerRepository
    {
        public EfCustomerRepository(ApplicationDbContext context)
            : base(context) { }

        public override IEnumerable<Customer> GetAll()
        {
            return _dbSet.Include(c => c.Orders).ToList();
        }

        public override async Task<IEnumerable<Customer>> GetAllAsync()
        {
            return await _dbSet.Include(c => c.Orders).ToListAsync();
        }

        public override Customer? GetById(int id)
        {
            return _dbSet.Include(c => c.Orders).FirstOrDefault(c => c.Id == id);
        }

        public override async Task<Customer?> GetByIdAsync(int id)
        {
            return await _dbSet.Include(c => c.Orders).FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
