using Microsoft.AspNetCore.Identity;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EmployeeIdentityRepository : IEmployeeIdentityRepository
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeRoleRepository _employeeRoleRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployeeIdentityRepository(
            IEmployeeRepository employeeRepository,
            IEmployeeRoleRepository employeeRoleRepository,
            UserManager<IdentityUser> userManager
        )
        {
            _employeeRepository = employeeRepository;
            _employeeRoleRepository = employeeRoleRepository;
            _userManager = userManager;
        }

        public async Task<bool> LinkEmployeeToIdentityUserAsync(int employeeId, string userId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            employee.UserId = userId;
            await _employeeRepository.UpdateAsync(employee);

            return await SyncEmployeeRoleWithIdentityRoleAsync(employeeId);
        }

        public async Task<bool> SyncEmployeeRoleWithIdentityRoleAsync(int employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null || string.IsNullOrEmpty(employee.UserId))
                return false;

            var user = await _userManager.FindByIdAsync(employee.UserId);
            if (user == null)
                return false;

            var employeeRole = await _employeeRoleRepository.GetByIdAsync(employee.EmployeeRoleId);
            if (employeeRole == null)
                return false;

            var userRoles = await _userManager.GetRolesAsync(user);

            if (userRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
            }

            var result = await _userManager.AddToRoleAsync(user, employeeRole.Role);

            return result.Succeeded;
        }

        public async Task<Employee?> GetEmployeeByIdentityUserIdAsync(string userId)
        {
            var employees = await _employeeRepository.GetAllAsync();
            return employees.FirstOrDefault(e => e.UserId == userId);
        }
    }
}
