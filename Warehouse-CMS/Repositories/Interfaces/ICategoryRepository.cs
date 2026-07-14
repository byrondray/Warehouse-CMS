using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface ICategoryRepository : IRepository<Category>, IAsyncRepository<Category> { }
}
