using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface IProductRepository : IRepository<Product>, IAsyncRepository<Product>
    {
        IEnumerable<Product> GetLowStock(int threshold);
        Task<IEnumerable<Product>> GetLowStockAsync(int threshold);
    }
}
