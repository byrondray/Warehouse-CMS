using System.ComponentModel.DataAnnotations;

namespace Warehouse_CMS.Models
{
    public class EmployeeRole
    {
        public int Id { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public List<Employee> Employees { get; set; } = new List<Employee>();
    }
}
