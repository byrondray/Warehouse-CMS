using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfEmployeeRoleRepository : EfCoreRepository<EmployeeRole>, IEmployeeRoleRepository
    {
        public EfEmployeeRoleRepository(ApplicationDbContext context)
            : base(context) { }

        public override IEnumerable<EmployeeRole> GetAll()
        {
            return _dbSet.Include(r => r.Employees).ToList();
        }

        public override EmployeeRole GetById(int id)
        {
            return _dbSet.Include(r => r.Employees).FirstOrDefault(r => r.Id == id);
        }

        public async Task<IEnumerable<EmployeeRole>> GetAllAsync()
        {
            return await _dbSet.Include(r => r.Employees).ToListAsync();
        }

        public async Task<EmployeeRole> GetByIdAsync(int id)
        {
            return await _dbSet.Include(r => r.Employees).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddAsync(EmployeeRole entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EmployeeRole entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
