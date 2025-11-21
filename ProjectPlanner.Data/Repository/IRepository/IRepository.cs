using System.Linq.Expressions;

namespace ProjectPlanner.Data.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        T? GetById(int id);

        T? GetFirstOrDefault(Expression<Func<T, bool>> filter);

        IEnumerable<T>? GetAll();

        IEnumerable<T>? GetAll(Expression<Func<T, bool>> filter);

        void Add(T entity);

        void Update(T entity);

        void Remove(T entity);

        void RemoveList(IEnumerable<T> entity);

        void RemoveAll();

        int CountAll();
    }
}