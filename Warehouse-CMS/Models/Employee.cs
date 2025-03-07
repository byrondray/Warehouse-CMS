using System.ComponentModel.DataAnnotations;

namespace Warehouse_CMS.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        public int EmployeeRoleId { get; set; }
        public EmployeeRole? EmployeeRole { get; set; }

        public List<Order> Orders { get; set; } = new List<Order>();
    }
}
