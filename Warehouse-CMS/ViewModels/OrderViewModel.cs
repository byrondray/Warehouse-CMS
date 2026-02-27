public class OrderViewModel
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<OrderItemViewModel> OrderItems { get; set; } = new();
    public decimal TotalAmount { get; set; }
}
