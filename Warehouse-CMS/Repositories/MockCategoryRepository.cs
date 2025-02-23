using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public class MockCategoryRepository : ICategoryRepository
    {
        private List<Category> _categories;

        public MockCategoryRepository()
        {
            _categories = new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    Description = "Electronic devices and accessories",
                },
                new Category
                {
                    Id = 2,
                    Name = "Furniture",
                    Description = "Office and home furniture",
                },
                new Category
                {
                    Id = 3,
                    Name = "Books",
                    Description = "Books and publications",
                },
            };
        }

        public IEnumerable<Category> GetAll() => _categories;

        public Category GetById(int id) => _categories.FirstOrDefault(c => c.Id == id);

        public void Add(Category category)
        {
            category.Id = _categories.Max(c => c.Id) + 1;
            _categories.Add(category);
        }

        public void Update(Category category)
        {
            var existing = _categories.FirstOrDefault(c => c.Id == category.Id);
            if (existing != null)
            {
                var index = _categories.IndexOf(existing);
                _categories[index] = category;
            }
        }

        public void Delete(int id)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                _categories.Remove(category);
            }
        }
    }
}
