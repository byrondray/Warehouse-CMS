using System.Collections.Generic;
using System.Linq;
using Warehouse_CMS.Models;
using Warehouse_CMS.Repositories;

namespace Warehouse_CMS.Repositories
{
    public class MockProductRepository : IProductRepository
    {
        private static List<Product> _products;
        private readonly ICategoryRepository _categoryRepository;

        public MockProductRepository(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;

            if (_products == null)
            {
                var constructionCategory = _categoryRepository.GetById(1);
                var toolsCategory = _categoryRepository.GetById(2);

                _products = new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        Name = "Drywall Sheet",
                        Description = "4' x 8' standard drywall sheet, 1/2\" thickness",
                        Price = 12.99m,
                        StockQuantity = 250,
                        CategoryId = constructionCategory.Id,
                        Category = constructionCategory,
                    },
                    new Product
                    {
                        Id = 2,
                        Name = "Hammer",
                        Description = "16 oz. claw hammer with fiberglass handle",
                        Price = 14.99m,
                        StockQuantity = 75,
                        CategoryId = toolsCategory.Id,
                        Category = toolsCategory,
                    },
                    new Product
                    {
                        Id = 3,
                        Name = "Concrete Mix",
                        Description = "60 lb. ready-to-use concrete mix",
                        Price = 6.50m,
                        StockQuantity = 320,
                        CategoryId = constructionCategory.Id,
                        Category = constructionCategory,
                    },
                };
            }
        }

        public IEnumerable<Product> GetAll()
        {
            System.Diagnostics.Debug.WriteLine($"Getting all products. Count: {_products.Count}");
            return _products;
        }

        public Product GetById(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            System.Diagnostics.Debug.WriteLine(
                $"Getting product by id {id}: {(product != null ? product.Name : "Not found")}"
            );
            return product;
        }

        public void Add(Product product)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Adding product: {product.Name}, Price: {product.Price}"
            );

            product.Id = _products.Max(p => p.Id) + 1;

            product.Category = _categoryRepository.GetById(product.CategoryId);

            _products.Add(product);

            System.Diagnostics.Debug.WriteLine($"Product added. Total products: {_products.Count}");
        }

        public void Update(Product product)
        {
            System.Diagnostics.Debug.WriteLine($"Updating product: {product.Id} - {product.Name}");

            var existing = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existing != null)
            {
                product.Category = _categoryRepository.GetById(product.CategoryId);

                var index = _products.IndexOf(existing);
                _products[index] = product;

                System.Diagnostics.Debug.WriteLine($"Product updated successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Product with ID {product.Id} not found for update"
                );
            }
        }

        public void Delete(int id)
        {
            System.Diagnostics.Debug.WriteLine($"Deleting product: {id}");

            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _products.Remove(product);
                System.Diagnostics.Debug.WriteLine(
                    $"Product deleted. Remaining products: {_products.Count}"
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Product with ID {id} not found for deletion");
            }
        }
    }
}
