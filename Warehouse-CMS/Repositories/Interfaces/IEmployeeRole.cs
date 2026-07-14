using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface IEmployeeRoleRepository : IRepository<EmployeeRole>, IAsyncRepository<EmployeeRole>
    {
        Task<EmployeeRole?> GetByNameAsync(string roleName);
    }
}
