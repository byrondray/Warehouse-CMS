using System;
using System.Collections.Generic;
using System.Linq;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public class MockCustomerRepository : ICustomerRepository
    {
        private static List<Customer> _customers;

        public MockCustomerRepository()
        {
            if (_customers == null)
            {
                _customers = new List<Customer>
                {
                    new Customer
                    {
                        Id = 1,
                        Name = "John Doe",
                        CreatedAt = DateTime.Now.AddDays(-30),
                        Orders = new List<Order>(),
                    },
                    new Customer
                    {
                        Id = 2,
                        Name = "Jane Smith",
                        CreatedAt = DateTime.Now.AddDays(-15),
                        Orders = new List<Order>(),
                    },
                    new Customer
                    {
                        Id = 3,
                        Name = "Bob Johnson",
                        CreatedAt = DateTime.Now.AddDays(-5),
                        Orders = new List<Order>(),
                    },
                };
            }
        }

        public IEnumerable<Customer> GetAll()
        {
            System.Diagnostics.Debug.WriteLine($"Getting all customers. Count: {_customers.Count}");
            return _customers;
        }

        public Customer GetById(int id)
        {
            var customer = _customers.FirstOrDefault(c => c.Id == id);
            System.Diagnostics.Debug.WriteLine(
                $"Getting customer by id {id}: {(customer != null ? customer.Name : "Not found")}"
            );
            return customer;
        }

        public void Add(Customer customer)
        {
            System.Diagnostics.Debug.WriteLine($"Adding customer: {customer.Name}");
            customer.Id = _customers.Any() ? _customers.Max(c => c.Id) + 1 : 1;
            if (customer.CreatedAt == default)
            {
                customer.CreatedAt = DateTime.Now;
            }
            _customers.Add(customer);
            System.Diagnostics.Debug.WriteLine(
                $"Customer added. Total customers: {_customers.Count}"
            );
        }

        public void Update(Customer customer)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Updating customer: {customer.Id} - {customer.Name}"
            );
            var existing = _customers.FirstOrDefault(c => c.Id == customer.Id);
            if (existing != null)
            {
                var index = _customers.IndexOf(existing);
                _customers[index] = customer;
                System.Diagnostics.Debug.WriteLine($"Customer updated successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Customer with ID {customer.Id} not found for update"
                );
            }
        }

        public void Delete(int id)
        {
            System.Diagnostics.Debug.WriteLine($"Deleting customer: {id}");
            var customer = _customers.FirstOrDefault(c => c.Id == id);
            if (customer != null)
            {
                _customers.Remove(customer);
                System.Diagnostics.Debug.WriteLine(
                    $"Customer deleted. Remaining customers: {_customers.Count}"
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Customer with ID {id} not found for deletion");
            }
        }
    }
}
