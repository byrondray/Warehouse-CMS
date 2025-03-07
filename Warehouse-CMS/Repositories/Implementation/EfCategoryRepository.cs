using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;
using Warehouse_CMS.Models;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfCategoryRepository : EfCoreRepository<Category>, ICategoryRepository
    {
        public EfCategoryRepository(ApplicationDbContext context)
            : base(context) { }
    }
}
