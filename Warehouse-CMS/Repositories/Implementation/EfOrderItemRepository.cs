using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfOrderItemRepository : EfCoreRepository<OrderItem>, IOrderItemRepository
    {
        public EfOrderItemRepository(ApplicationDbContext context)
            : base(context) { }

        public override IEnumerable<OrderItem> GetAll()
        {
            return _dbSet.Include(i => i.Order).Include(i => i.Product).ToList();
        }

        public override OrderItem GetById(int id)
        {
            return _dbSet
                .Include(i => i.Order)
                .Include(i => i.Product)
                .FirstOrDefault(i => i.Id == id);
        }

        public IEnumerable<OrderItem> GetByOrderId(int orderId)
        {
            return _dbSet.Include(i => i.Product).Where(i => i.OrderId == orderId).ToList();
        }
    }
}
