using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfOrderStatusRepository : EfCoreRepository<OrderStatus>, IOrderStatusRepository
    {
        public EfOrderStatusRepository(ApplicationDbContext context)
            : base(context) { }

        public override IEnumerable<OrderStatus> GetAll()
        {
            return _dbSet.Include(s => s.Orders).ToList();
        }

        public override OrderStatus GetById(int id)
        {
            return _dbSet.Include(s => s.Orders).FirstOrDefault(s => s.Id == id);
        }
    }
}
