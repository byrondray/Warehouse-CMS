using System.Collections.Generic;
using System.Threading.Tasks;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface IEmployeeRoleRepository
    {
        IEnumerable<EmployeeRole> GetAll();
        EmployeeRole GetById(int id);
        void Add(EmployeeRole entity);
        void Update(EmployeeRole entity);
        void Delete(int id);

        Task<IEnumerable<EmployeeRole>> GetAllAsync();
        Task<EmployeeRole> GetByIdAsync(int id);
        Task AddAsync(EmployeeRole entity);
        Task UpdateAsync(EmployeeRole entity);
        Task DeleteAsync(int id);
    }
}
