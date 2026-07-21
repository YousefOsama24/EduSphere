using EduSphere.Models;
using System.Linq.Expressions;

namespace EduSphere.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        #region Get

        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = false,
            CancellationToken cancellationToken = default,
            int skip = 0,
            int take = 0);

        Task<T?> GetOneAsync(
            Expression<Func<T, bool>> filter,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = false,
            CancellationToken cancellationToken = default);

        Task<T?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        #endregion

        #region Exists & Aggregation

        Task<bool> AnyAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        #endregion

        #region Create

        Task CreateAsync(
            T entity,
            CancellationToken cancellationToken = default);

        Task CreateRangeAsync(
            IEnumerable<T> entities,
            CancellationToken cancellationToken = default);

        #endregion

        #region Update

        void Update(T entity);

        void UpdateRange(IEnumerable<T> entities);

        #endregion

        #region Delete

        void Delete(T entity);

        void DeleteRange(IEnumerable<T> entities);

        #endregion

        #region Save

        Task CommitAsync(
            CancellationToken cancellationToken = default);

        #endregion
    }
}