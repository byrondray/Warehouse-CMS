namespace Warehouse_CMS.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string ContactPerson { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public List<Product>? Products { get; set; }
    }
}
