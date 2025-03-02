using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories
{
    public interface ISupplierRepository
    {
        IEnumerable<Supplier> GetAll();
        Supplier GetById(int id);
        void Add(Supplier supplier);
        void Update(Supplier supplier);
        void Delete(int id);
    }
}
