using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfEmployeeRepository : EfCoreRepository<Employee>, IEmployeeRepository
    {
        public EfEmployeeRepository(ApplicationDbContext context)
            : base(context) { }

        public override IEnumerable<Employee> GetAll()
        {
            return _dbSet.Include(e => e.EmployeeRole).Include(e => e.Orders).ToList();
        }

        public override Employee? GetById(int id)
        {
            return _dbSet
                .Include(e => e.EmployeeRole)
                .Include(e => e.Orders)
                .FirstOrDefault(e => e.Id == id);
        }

        public override async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _dbSet.Include(e => e.EmployeeRole).Include(e => e.Orders).ToListAsync();
        }

        public override async Task<Employee?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(e => e.EmployeeRole)
                .Include(e => e.Orders)
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
