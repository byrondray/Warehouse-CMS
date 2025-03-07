using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface IEmployeeRoleRepository
    {
        IEnumerable<EmployeeRole> GetAll();
        EmployeeRole GetById(int id);
        void Add(EmployeeRole employeeRole);
        void Update(EmployeeRole employeeRole);
        void Delete(int id);
    }
}
