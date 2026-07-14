using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface IEmployeeRepository : IRepository<Employee>, IAsyncRepository<Employee> { }
}
