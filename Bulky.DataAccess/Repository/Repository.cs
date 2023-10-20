using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            dbSet = _db.Set<T>();
        }
        public void Add(T entity) => dbSet.Add(entity);

        public T Get(Expression<Func<T, bool>> predicate, IEnumerable<string> includeProperties)
        {
            return IncludeProperties(dbSet.Where(predicate), includeProperties).FirstOrDefault();
        }

        public IEnumerable<T> GetAll(IEnumerable<string> includeProperties)
        {
            return IncludeProperties(dbSet, includeProperties).ToList();
        }

        public void Remove(T entity) => dbSet.Remove(entity);

        public void RemoveRange(IEnumerable<T> entities) => dbSet.RemoveRange(entities);

        private static IQueryable<T> IncludeProperties(IQueryable<T> queryable, IEnumerable<string> properties)
        {
            foreach (var property in properties ?? Enumerable.Empty<string>())
                queryable = queryable.Include(property);
            return queryable;
        }
    }
}
