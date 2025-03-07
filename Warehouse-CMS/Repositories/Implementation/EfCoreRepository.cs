// EfCoreRepository.cs
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Warehouse_CMS.Data;

namespace Warehouse_CMS.Repositories.Implementation
{
    public class EfCoreRepository<T>
        where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public EfCoreRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public virtual T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public virtual void Add(T entity)
        {
            _dbSet.Add(entity);
            _context.SaveChanges();
        }

        public virtual void Update(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public virtual void Delete(int id)
        {
            var entity = GetById(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                _context.SaveChanges();
            }
        }
    }
}
