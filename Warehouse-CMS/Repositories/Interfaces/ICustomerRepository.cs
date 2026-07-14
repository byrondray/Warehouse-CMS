using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface ICustomerRepository : IRepository<Customer>, IAsyncRepository<Customer> { }
}
