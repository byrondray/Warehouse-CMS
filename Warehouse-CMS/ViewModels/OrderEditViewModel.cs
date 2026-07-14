using System.ComponentModel.DataAnnotations;

namespace Warehouse_CMS.ViewModels
{
    /// <summary>
    /// Bound by OrderController.Edit. Deliberately narrow: order editing only changes the
    /// status, so binding the full Order entity (with TotalAmount, CustomerId, EmployeeId,
    /// OrderItems, …) would be an overposting risk.
    /// </summary>
    public class OrderEditViewModel
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a status.")]
        public int OrderStatusId { get; set; }
    }
}
