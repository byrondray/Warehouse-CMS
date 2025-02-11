using Warehouse_CMS.Repositories;

public class MockProductRepository : IProductRepository
{
    private List<Product> _products;
    private readonly ICategoryRepository _categoryRepository;

    public MockProductRepository(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
        var electronicsCategory = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic devices and accessories",
        };
        var furnitureCategory = new Category
        {
            Id = 2,
            Name = "Furniture",
            Description = "Office and home furniture",
        };

        _products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Description = "High-end laptop",
                Price = 999.99m,
                StockQuantity = 10,
                CategoryId = electronicsCategory.Id,
                Category = electronicsCategory,
            },
            new Product
            {
                Id = 2,
                Name = "Desk Chair",
                Description = "Ergonomic office chair",
                Price = 199.99m,
                StockQuantity = 15,
                CategoryId = furnitureCategory.Id,
                Category = furnitureCategory,
            },
            new Product
            {
                Id = 3,
                Name = "Monitor",
                Description = "27-inch 4K monitor",
                Price = 299.99m,
                StockQuantity = 5,
                CategoryId = electronicsCategory.Id,
                Category = electronicsCategory,
            },
        };
    }

    public IEnumerable<Product> GetAll() => _products;

    public Product GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

    public void Add(Product product)
    {
        product.Id = _products.Max(p => p.Id) + 1;
        product.Category = _categoryRepository.GetById(product.CategoryId);
        _products.Add(product);
    }

    public void Update(Product product)
    {
        var existing = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existing != null)
        {
            product.Category = _categoryRepository.GetById(product.CategoryId);
            var index = _products.IndexOf(existing);
            _products[index] = product;
        }
    }

    public void Delete(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
        }
    }
}
