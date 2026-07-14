using Warehouse_CMS.Models;
using Warehouse_CMS.ViewModels;

namespace Warehouse_CMS.Repositories
{
    public interface IProductRepository : IRepository<Product>, IAsyncRepository<Product>
    {
        IEnumerable<Product> GetLowStock(int threshold);
        Task<IEnumerable<Product>> GetLowStockAsync(int threshold);

        /// <summary>
        /// Returns one page of products projected to <see cref="ProductViewModel"/> at the
        /// query level (no full entity graph or in-memory projection).
        /// </summary>
        Task<PagedResult<ProductViewModel>> GetPagedAsync(int pageNumber, int pageSize);
    }
}
