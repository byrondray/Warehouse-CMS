using Microsoft.AspNetCore.Identity;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class RoleManagementRepository : IRoleManagementRepository
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmployeeRoleRepository _employeeRoleRepository;

        public RoleManagementRepository(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            IEmployeeRoleRepository employeeRoleRepository
        )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _employeeRoleRepository = employeeRoleRepository;
        }

        public async Task SyncEmployeeRolesWithIdentityRolesAsync()
        {
            var employeeRoles = await _employeeRoleRepository.GetAllAsync();

            foreach (var role in employeeRoles)
            {
                if (!await _roleManager.RoleExistsAsync(role.Role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role.Role));
                }
            }
        }

        public async Task<IList<IdentityRole>> GetAllIdentityRolesAsync()
        {
            return _roleManager.Roles.ToList();
        }

        public async Task<IdentityRole?> GetIdentityRoleByIdAsync(string roleId)
        {
            return await _roleManager.FindByIdAsync(roleId);
        }

        public async Task<bool> CreateIdentityRoleAsync(string roleName)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            return result.Succeeded;
        }

        public async Task<bool> UpdateIdentityRoleAsync(string roleId, string newRoleName)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return false;

            role.Name = newRoleName;
            var result = await _roleManager.UpdateAsync(role);
            return result.Succeeded;
        }

        public async Task<bool> DeleteIdentityRoleAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return false;

            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded;
        }

        public async Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName)
        {
            return await _userManager.GetUsersInRoleAsync(roleName);
        }

        public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<bool> RemoveUserFromRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            return result.Succeeded;
        }

        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new List<string>();

            return await _userManager.GetRolesAsync(user);
        }
    }
}
