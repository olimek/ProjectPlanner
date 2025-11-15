using Microsoft.EntityFrameworkCore;
using ProjectPlanner.Data.Contexts;
using ProjectPlanner.Data.Repository.IRepository;
using System.Linq.Expressions;

namespace ProjectPlanner.Data.Repository
{
    public abstract class Repository<T> : IRepository<T> where T : class
    {
        private readonly ProjectContext _db;
        internal DbSet<T> dbSet;

        public Repository(ProjectContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }

        public virtual void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public virtual IEnumerable<T>? GetAll()
        {
            IQueryable<T> query = dbSet;
            return query.ToList();
        }

        public virtual T? GetFirstOrDefault(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = dbSet;
            query = query.Where(filter);
            return query.FirstOrDefault();
        }

        public virtual void Remove(T entity)
        {
            dbSet.Remove(entity);
        }
        public virtual int CountAll()
        {
            return dbSet.Count();
        }
        public virtual void RemoveAll()
        {
            var entities = GetAll();
            if (entities is not null) 
                RemoveList(entities);
        }

        public virtual void RemoveList(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }

        public abstract void Update(T entity);
    }
}
