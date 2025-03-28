using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Warehouse_CMS.Models.ViewModels;
using Warehouse_CMS.Repositories;

namespace Warehouse_CMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly IRoleManagementRepository _roleRepository;
        private readonly IEmployeeRoleRepository _employeeRoleRepository;
        private readonly UserManager<IdentityUser> _userManager;

        public RolesController(
            IRoleManagementRepository roleRepository,
            IEmployeeRoleRepository employeeRoleRepository,
            UserManager<IdentityUser> userManager
        )
        {
            _roleRepository = roleRepository;
            _employeeRoleRepository = employeeRoleRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            await _roleRepository.SyncEmployeeRolesWithIdentityRolesAsync();

            var identityRoles = await _roleRepository.GetAllIdentityRolesAsync();
            var employeeRoles = await _employeeRoleRepository.GetAllAsync();

            var viewModel = new RolesIndexViewModel
            {
                IdentityRoles = identityRoles,
                EmployeeRoles = employeeRoles,
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var identityRoleCreated = await _roleRepository.CreateIdentityRoleAsync(
                    model.RoleName
                );

                if (identityRoleCreated)
                {
                    var employeeRole = new Warehouse_CMS.Models.EmployeeRole
                    {
                        Role = model.RoleName,
                        Description = model.Description,
                    };

                    await _employeeRoleRepository.AddAsync(employeeRole);

                    TempData["SuccessMessage"] = $"Role '{model.RoleName}' created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Role creation failed. Please try again.");
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var identityRole = await _roleRepository.GetIdentityRoleByIdAsync(id);
            if (identityRole == null)
            {
                return NotFound();
            }

            var employeeRole = (await _employeeRoleRepository.GetAllAsync()).FirstOrDefault(r =>
                r.Role == identityRole.Name
            );

            var viewModel = new EditRoleViewModel
            {
                RoleId = identityRole.Id,
                RoleName = identityRole.Name,
                Description = employeeRole?.Description ?? "",
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditRoleViewModel model)
        {
            if (id != model.RoleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var identityRole = await _roleRepository.GetIdentityRoleByIdAsync(id);
                if (identityRole == null)
                {
                    return NotFound();
                }

                var oldRoleName = identityRole.Name;

                var updateResult = await _roleRepository.UpdateIdentityRoleAsync(
                    id,
                    model.RoleName
                );

                if (updateResult)
                {
                    var employeeRole = (await _employeeRoleRepository.GetAllAsync()).FirstOrDefault(
                        r => r.Role == oldRoleName
                    );

                    if (employeeRole != null)
                    {
                        employeeRole.Role = model.RoleName;
                        employeeRole.Description = model.Description;
                        await _employeeRoleRepository.UpdateAsync(employeeRole);
                    }

                    TempData["SuccessMessage"] = $"Role '{model.RoleName}' updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Role update failed. Please try again.");
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var identityRole = await _roleRepository.GetIdentityRoleByIdAsync(id);
            if (identityRole == null)
            {
                return NotFound();
            }

            var employeeRole = (await _employeeRoleRepository.GetAllAsync()).FirstOrDefault(r =>
                r.Role == identityRole.Name
            );

            var viewModel = new DeleteRoleViewModel
            {
                RoleId = identityRole.Id,
                RoleName = identityRole.Name,
                Description = employeeRole?.Description ?? "",
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var identityRole = await _roleRepository.GetIdentityRoleByIdAsync(id);
            if (identityRole == null)
            {
                return NotFound();
            }

            var roleName = identityRole.Name;

            var deleteResult = await _roleRepository.DeleteIdentityRoleAsync(id);

            if (deleteResult)
            {
                var employeeRole = (await _employeeRoleRepository.GetAllAsync()).FirstOrDefault(r =>
                    r.Role == roleName
                );

                if (employeeRole != null)
                {
                    await _employeeRoleRepository.DeleteAsync(employeeRole.Id);
                }

                TempData["SuccessMessage"] = $"Role '{roleName}' deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Role deletion failed. It may be assigned to users.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ManageUsers(string id)
        {
            var role = await _roleRepository.GetIdentityRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var usersInRole = await _roleRepository.GetUsersInRoleAsync(role.Name);
            var allUsers = _userManager.Users.ToList();

            var viewModel = new ManageUsersInRoleViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Users = allUsers
                    .Select(u => new UserRoleViewModel
                    {
                        UserId = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        IsSelected = usersInRole.Any(ur => ur.Id == u.Id),
                    })
                    .ToList(),
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUsers(ManageUsersInRoleViewModel model)
        {
            var role = await _roleRepository.GetIdentityRoleByIdAsync(model.RoleId);
            if (role == null)
            {
                return NotFound();
            }

            var roleName = role.Name;
            var usersInRole = await _roleRepository.GetUsersInRoleAsync(roleName);

            foreach (var user in model.Users)
            {
                var isInRole = usersInRole.Any(u => u.Id == user.UserId);

                if (user.IsSelected && !isInRole)
                {
                    await _roleRepository.AddUserToRoleAsync(user.UserId, roleName);
                }
                else if (!user.IsSelected && isInRole)
                {
                    await _roleRepository.RemoveUserFromRoleAsync(user.UserId, roleName);
                }
            }

            TempData["SuccessMessage"] =
                $"User assignments for role '{roleName}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
