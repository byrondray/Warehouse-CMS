using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfOrderRepository : EfCoreRepository<Order>, IOrderRepository
    {
        public EfOrderRepository(ApplicationDbContext context)
            : base(context) { }

        private IQueryable<Order> WithFullGraph() =>
            _dbSet
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product);

        public override IEnumerable<Order> GetAll()
        {
            return WithFullGraph().ToList();
        }

        public override async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await WithFullGraph().ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllForListAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public override Order? GetById(int id)
        {
            return WithFullGraph().FirstOrDefault(o => o.Id == id);
        }

        public override async Task<Order?> GetByIdAsync(int id)
        {
            return await WithFullGraph().FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
