using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfOrderRepository : EfCoreRepository<Order>, IOrderRepository
    {
        public EfOrderRepository(ApplicationDbContext context)
            : base(context) { }

        public override IEnumerable<Order> GetAll()
        {
            return _dbSet
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .ToList();
        }

        public override Order GetById(int id)
        {
            return _dbSet
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == id);
        }
    }
}
