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

        public override Employee GetById(int id)
        {
            return _dbSet
                .Include(e => e.EmployeeRole)
                .Include(e => e.Orders)
                .FirstOrDefault(e => e.Id == id);
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _dbSet.Include(e => e.EmployeeRole).Include(e => e.Orders).ToListAsync();
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(e => e.EmployeeRole)
                .Include(e => e.Orders)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(Employee employee)
        {
            await _dbSet.AddAsync(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            _dbSet.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var employee = await _dbSet.FindAsync(id);
            if (employee != null)
            {
                _dbSet.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }
    }
}
