using Microsoft.AspNetCore.Identity;

namespace Warehouse_CMS.Repositories
{
    public interface IRoleManagementRepository
    {
        Task SyncEmployeeRolesWithIdentityRolesAsync();
        Task<IList<IdentityRole>> GetAllIdentityRolesAsync();
        Task<IdentityRole?> GetIdentityRoleByIdAsync(string roleId);
        Task<bool> CreateIdentityRoleAsync(string roleName);
        Task<bool> UpdateIdentityRoleAsync(string roleId, string newRoleName);
        Task<bool> DeleteIdentityRoleAsync(string roleId);
        Task<IList<IdentityUser>> GetUsersInRoleAsync(string roleName);
        Task<bool> AddUserToRoleAsync(string userId, string roleName);
        Task<bool> RemoveUserFromRoleAsync(string userId, string roleName);
        Task<IList<string>> GetUserRolesAsync(string userId);
    }
}
