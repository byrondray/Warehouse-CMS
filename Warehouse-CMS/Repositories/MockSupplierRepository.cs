namespace Warehouse_CMS.Repositories
{
    public class MockSupplierRepository : ISupplierRepository
    {
        private List<Supplier> _suppliers;

        public MockSupplierRepository()
        {
            _suppliers = new List<Supplier>
            {
                new Supplier
                {
                    Id = 1,
                    Name = "Tech Supplies Co",
                    ContactPerson = "Mike Johnson",
                    Email = "mike@techsupplies.com",
                    Phone = "555-0123",
                    Products = new List<Product>(),
                },
                new Supplier
                {
                    Id = 2,
                    Name = "Office Furniture Ltd",
                    ContactPerson = "Sarah Williams",
                    Email = "sarah@officefurniture.com",
                    Phone = "555-0456",
                    Products = new List<Product>(),
                },
            };
        }

        public IEnumerable<Supplier> GetAll() => _suppliers;

        public Supplier GetById(int id) => _suppliers.FirstOrDefault(s => s.Id == id);

        public void Add(Supplier supplier)
        {
            supplier.Id = _suppliers.Max(s => s.Id) + 1;
            _suppliers.Add(supplier);
        }

        public void Update(Supplier supplier)
        {
            var existing = _suppliers.FirstOrDefault(s => s.Id == supplier.Id);
            if (existing != null)
            {
                var index = _suppliers.IndexOf(existing);
                _suppliers[index] = supplier;
            }
        }

        public void Delete(int id)
        {
            var supplier = _suppliers.FirstOrDefault(s => s.Id == id);
            if (supplier != null)
            {
                _suppliers.Remove(supplier);
            }
        }
    }
}
