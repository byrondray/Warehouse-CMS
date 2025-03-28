using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Warehouse_CMS.Models.ViewModels
{
    public class RolesIndexViewModel
    {
        public IEnumerable<IdentityRole> IdentityRoles { get; set; } = new List<IdentityRole>();
        public IEnumerable<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    }

    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
    }

    public class EditRoleViewModel
    {
        public string RoleId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;
    }

    public class DeleteRoleViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class ManageUsersInRoleViewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<UserRoleViewModel> Users { get; set; } = new List<UserRoleViewModel>();
    }
}
