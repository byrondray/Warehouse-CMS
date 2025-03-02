using System.Collections.Generic;
using System.Linq;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public class MockCategoryRepository : ICategoryRepository
    {
        private static List<Category> _categories;

        public MockCategoryRepository()
        {
            if (_categories == null)
            {
                _categories = new List<Category>
                {
                    new Category
                    {
                        Id = 1,
                        Name = "Construction Materials",
                        Description = "Building and construction materials",
                    },
                    new Category
                    {
                        Id = 2,
                        Name = "Tools",
                        Description = "Construction and building tools",
                    },
                    new Category
                    {
                        Id = 3,
                        Name = "Safety Equipment",
                        Description = "Protective gear and safety supplies",
                    },
                };
            }
        }

        public IEnumerable<Category> GetAll()
        {
            System.Diagnostics.Debug.WriteLine(
                $"Getting all categories. Count: {_categories.Count}"
            );
            return _categories;
        }

        public Category GetById(int id)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            System.Diagnostics.Debug.WriteLine(
                $"Getting category by id {id}: {(category != null ? category.Name : "Not found")}"
            );
            return category;
        }

        public void Add(Category category)
        {
            System.Diagnostics.Debug.WriteLine($"Adding category: {category.Name}");
            category.Id = _categories.Max(c => c.Id) + 1;
            _categories.Add(category);
            System.Diagnostics.Debug.WriteLine(
                $"Category added. Total categories: {_categories.Count}"
            );
        }

        public void Update(Category category)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Updating category: {category.Id} - {category.Name}"
            );
            var existing = _categories.FirstOrDefault(c => c.Id == category.Id);
            if (existing != null)
            {
                var index = _categories.IndexOf(existing);
                _categories[index] = category;
                System.Diagnostics.Debug.WriteLine($"Category updated successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Category with ID {category.Id} not found for update"
                );
            }
        }

        public void Delete(int id)
        {
            System.Diagnostics.Debug.WriteLine($"Deleting category: {id}");
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                _categories.Remove(category);
                System.Diagnostics.Debug.WriteLine(
                    $"Category deleted. Remaining categories: {_categories.Count}"
                );
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Category with ID {id} not found for deletion");
            }
        }
    }
}
