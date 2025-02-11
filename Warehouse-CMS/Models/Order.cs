public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerName { get; set; }
    public string Status { get; set; }
    public List<OrderItem> OrderItems { get; set; }
    public decimal TotalAmount { get; set; }
}
