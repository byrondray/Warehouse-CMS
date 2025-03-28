// using System.Collections.Generic;
// using System.Linq;
// using Warehouse_CMS.Models;

// namespace Warehouse_CMS.Repositories
// {
//     public class MockEmployeeRoleRepository : IEmployeeRoleRepository
//     {
//         private static List<EmployeeRole> _employeeRoles;

//         public MockEmployeeRoleRepository()
//         {
//             if (_employeeRoles == null)
//             {
//                 _employeeRoles = new List<EmployeeRole>
//                 {
//                     new EmployeeRole
//                     {
//                         Id = 1,
//                         Role = "Sales Associate",
//                         Description = "Handles customer orders and sales",
//                         Employees = new List<Employee>(),
//                     },
//                     new EmployeeRole
//                     {
//                         Id = 2,
//                         Role = "Manager",
//                         Description = "Oversees warehouse operations",
//                         Employees = new List<Employee>(),
//                     },
//                     new EmployeeRole
//                     {
//                         Id = 3,
//                         Role = "Warehouse Staff",
//                         Description = "Handles inventory and shipping",
//                         Employees = new List<Employee>(),
//                     },
//                 };
//             }
//         }

//         public IEnumerable<EmployeeRole> GetAll()
//         {
//             System.Diagnostics.Debug.WriteLine(
//                 $"Getting all employee roles. Count: {_employeeRoles.Count}"
//             );
//             return _employeeRoles;
//         }

//         public EmployeeRole GetById(int id)
//         {
//             var role = _employeeRoles.FirstOrDefault(r => r.Id == id);
//             System.Diagnostics.Debug.WriteLine(
//                 $"Getting employee role by id {id}: {(role != null ? role.Role : "Not found")}"
//             );
//             return role;
//         }

//         public void Add(EmployeeRole employeeRole)
//         {
//             System.Diagnostics.Debug.WriteLine($"Adding employee role: {employeeRole.Role}");
//             employeeRole.Id = _employeeRoles.Any() ? _employeeRoles.Max(r => r.Id) + 1 : 1;
//             _employeeRoles.Add(employeeRole);
//             System.Diagnostics.Debug.WriteLine(
//                 $"Employee role added. Total roles: {_employeeRoles.Count}"
//             );
//         }

//         public void Update(EmployeeRole employeeRole)
//         {
//             System.Diagnostics.Debug.WriteLine(
//                 $"Updating employee role: {employeeRole.Id} - {employeeRole.Role}"
//             );
//             var existing = _employeeRoles.FirstOrDefault(r => r.Id == employeeRole.Id);
//             if (existing != null)
//             {
//                 var index = _employeeRoles.IndexOf(existing);
//                 _employeeRoles[index] = employeeRole;
//                 System.Diagnostics.Debug.WriteLine($"Employee role updated successfully");
//             }
//             else
//             {
//                 System.Diagnostics.Debug.WriteLine(
//                     $"Employee role with ID {employeeRole.Id} not found for update"
//                 );
//             }
//         }

//         public void Delete(int id)
//         {
//             System.Diagnostics.Debug.WriteLine($"Deleting employee role: {id}");
//             var role = _employeeRoles.FirstOrDefault(r => r.Id == id);
//             if (role != null)
//             {
//                 _employeeRoles.Remove(role);
//                 System.Diagnostics.Debug.WriteLine(
//                     $"Employee role deleted. Remaining roles: {_employeeRoles.Count}"
//                 );
//             }
//             else
//             {
//                 System.Diagnostics.Debug.WriteLine(
//                     $"Employee role with ID {id} not found for deletion"
//                 );
//             }
//         }
//     }
// }
