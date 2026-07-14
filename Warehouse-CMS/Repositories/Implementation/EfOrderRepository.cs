using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;
using Warehouse_CMS.ViewModels;

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

        public async Task<PagedResult<Order>> GetPagedForListAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 20;

            var baseQuery = _dbSet.AsNoTracking().OrderByDescending(o => o.OrderDate);
            var totalCount = await baseQuery.CountAsync();

            var items = await baseQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(o => o.Customer)
                .Include(o => o.OrderStatus)
                .ToListAsync();

            return new PagedResult<Order>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
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
