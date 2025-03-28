using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface IEmployeeIdentityRepository
    {
        Task<bool> LinkEmployeeToIdentityUserAsync(int employeeId, string userId);
        Task<bool> SyncEmployeeRoleWithIdentityRoleAsync(int employeeId);
        Task<Employee?> GetEmployeeByIdentityUserIdAsync(string userId);
    }
}
