namespace Warehouse_CMS.Models
{
    /// <summary>
    /// Central definition of role names and which roles are privileged.
    /// Privileged roles grant elevated access (role management, environment info, product CRUD)
    /// and must never be self-assignable during anonymous registration — they can only be
    /// granted by an existing Admin/Manager through the Roles UI.
    /// </summary>
    public static class RoleConstants
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";

        /// <summary>
        /// Roles a user is not allowed to assign to themselves at registration time.
        /// </summary>
        public static readonly IReadOnlySet<string> PrivilegedRoles = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase
        )
        {
            Admin,
            Manager,
        };

        public static bool IsPrivileged(string? roleName) =>
            !string.IsNullOrEmpty(roleName) && PrivilegedRoles.Contains(roleName);
    }
}
