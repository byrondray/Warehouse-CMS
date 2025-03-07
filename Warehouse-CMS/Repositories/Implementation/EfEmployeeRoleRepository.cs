using System.Collections.Generic;
using System.Linq;
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
    }
}
