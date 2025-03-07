using System.Collections.Generic;
using System.Linq;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public class MockSupplierRepository : ISupplierRepository
    {
        private static List<Supplier> _suppliers;

        public MockSupplierRepository()
        {
            if (_suppliers == null)
            {
                _suppliers = new List<Supplier>
                {
                    new Supplier
                    {
                        Id = 1,
                        Name = "Home Depot",
                        ContactPerson = "Mike Johnson",
                        Email = "mike@homedepot.com",
                        Phone = "555-0123",
                        Products = new List<Product>(),
                    },
                    new Supplier
                    {
                        Id = 2,
                        Name = "Lowes",
                        ContactPerson = "Sarah Williams",
                        Email = "sarah@lowes.com",
                        Phone = "555-0456",
                        Products = new List<Product>(),
                    },
                };
            }
        }

        public IEnumerable<Supplier> GetAll()
        {
            System.Diagnostics.Debug.WriteLine($"Getting all suppliers. Count: {_suppliers.Count}");
            return _suppliers;
        }

        public Supplier GetById(int id)
        {
            var supplier = _suppliers.FirstOrDefault(s => s.Id == id);
            System.Diagnostics.Debug.WriteLine(
                $"Getting supplier by id {id}: {(supplier != null ? supplier.Name : "Not found")}"
            );
            return supplier;
        }

        public void Add(Supplier supplier)
        {
            System.Diagnostics.Debug.WriteLine($"Adding supplier: {supplier.Name}");
            supplier.Id = _suppliers.Max(s => s.Id) + 1;

            if (supplier.Products == null)
            {
                supplier.Products = new List<Product>();
            }

            _suppliers.Add(supplier);
            System.Diagnostics.Debug.WriteLine(
                $"Supplier added. Total suppliers: {_suppliers.Count}"
            );
        }

        public void Update(Supplier supplier)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Updating supplier: {supplier.Id} - {supplier.Name}"
            );
            var existing = _suppliers.FirstOrDefault(s => s.Id == supplier.Id);
            if (existing != null)
            {
                // Preserve existing product relationships if not in the updated supplier
                if (supplier.Products == null)
                {
                    supplier.Products = existing.Products;
                }

                var index = _suppliers.IndexOf(existing);
                _suppliers[index] = supplier;
                System.Diagnostics.Debug.WriteLine($"Supplier updated successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Supplier with ID {supplier.Id} not found for update"
                );
            }
        }

        public void Delete(int id)
        {
            System.Diagnostics.Debug.WriteLine($"Deleting supplier: {id}");
            var supplier = _suppliers.FirstOrDefault(s => s.Id == id);
            if (supplier != null)
            {
                _suppliers.Remove(supplier);
                System.Diagnostics.Debug.WriteLine(
                    $"Supplier deleted. Remaining suppliers: {_suppliers.Count}"
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Supplier with ID {id} not found for deletion");
            }
        }
    }
}
