using System;
using System.Collections.Generic;
using System.Linq;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public class MockEmployeeRepository : IEmployeeRepository
    {
        private static List<Employee> _employees;
        private readonly IEmployeeRoleRepository _employeeRoleRepository;

        public MockEmployeeRepository(IEmployeeRoleRepository employeeRoleRepository)
        {
            _employeeRoleRepository = employeeRoleRepository;

            if (_employees == null)
            {
                var salesRole = _employeeRoleRepository.GetById(1);
                var managerRole = _employeeRoleRepository.GetById(2);

                _employees = new List<Employee>
                {
                    new Employee
                    {
                        Id = 1,
                        Name = "Alice Brown",
                        StartDate = DateTime.Now.AddYears(-2),
                        EmployeeRoleId = salesRole.Id,
                        EmployeeRole = salesRole,
                        Orders = new List<Order>(),
                    },
                    new Employee
                    {
                        Id = 2,
                        Name = "Charlie Davis",
                        StartDate = DateTime.Now.AddYears(-5),
                        EmployeeRoleId = managerRole.Id,
                        EmployeeRole = managerRole,
                        Orders = new List<Order>(),
                    },
                };
            }
        }

        public IEnumerable<Employee> GetAll()
        {
            System.Diagnostics.Debug.WriteLine($"Getting all employees. Count: {_employees.Count}");
            return _employees;
        }

        public Employee GetById(int id)
        {
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            System.Diagnostics.Debug.WriteLine(
                $"Getting employee by id {id}: {(employee != null ? employee.Name : "Not found")}"
            );
            return employee;
        }

        public void Add(Employee employee)
        {
            System.Diagnostics.Debug.WriteLine($"Adding employee: {employee.Name}");
            employee.Id = _employees.Any() ? _employees.Max(e => e.Id) + 1 : 1;
            employee.EmployeeRole = _employeeRoleRepository.GetById(employee.EmployeeRoleId);
            _employees.Add(employee);
            System.Diagnostics.Debug.WriteLine(
                $"Employee added. Total employees: {_employees.Count}"
            );
        }

        public void Update(Employee employee)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Updating employee: {employee.Id} - {employee.Name}"
            );
            var existing = _employees.FirstOrDefault(e => e.Id == employee.Id);
            if (existing != null)
            {
                employee.EmployeeRole = _employeeRoleRepository.GetById(employee.EmployeeRoleId);
                var index = _employees.IndexOf(existing);
                _employees[index] = employee;
                System.Diagnostics.Debug.WriteLine($"Employee updated successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Employee with ID {employee.Id} not found for update"
                );
            }
        }

        public void Delete(int id)
        {
            System.Diagnostics.Debug.WriteLine($"Deleting employee: {id}");
            var employee = _employees.FirstOrDefault(e => e.Id == id);
            if (employee != null)
            {
                _employees.Remove(employee);
                System.Diagnostics.Debug.WriteLine(
                    $"Employee deleted. Remaining employees: {_employees.Count}"
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Employee with ID {id} not found for deletion");
            }
        }
    }
}
